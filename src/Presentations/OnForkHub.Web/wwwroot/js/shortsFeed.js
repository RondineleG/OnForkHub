// Vertical shorts feed: one snap-scrolled item per torrent. Only the visible item plays; the next one is preloaded
// once it does, and anything further away is released so bandwidth goes to what the viewer is watching.
// Gestures on the video: tap toggles play, double-tap on the left/right third seeks, press and hold plays at 2×.
import { startDownload, stopDownload, getTorrentStats } from './webtorrentService.js';

const PRELOAD_DISTANCE = 1;
const VISIBLE_RATIO = 0.6;
const STATS_INTERVAL_MS = 1000;
const AMBIENT_INTERVAL_MS = 250;
const DOUBLE_TAP_MS = 280;
const SEEK_CHAIN_MS = 700;
const LONG_PRESS_MS = 420;
const TAP_SLOP_PX = 10;
const SEEK_STEP_S = 10;
const KEY_SEEK_STEP_S = 5;
const BOOST_RATE = 2;
const RATES = [0.5, 0.75, 1, 1.25, 1.5, 2];
const PREFS_KEY = 'onforkhub.player.v2';
const IDLE_MS = 2500;
const THEME_KEY = 'onforkhub.theme';
const THEME_COLORS = { dark: '#0a0806', light: '#F2EBD9' };

let feed = null;

export function initShortsFeed(root) {
    disposeShortsFeed();

    const prefs = loadPrefs();
    const scroller = root.querySelector('.shorts');
    const items = [...scroller.querySelectorAll('.short')].map((element, index) => ({
        element,
        index,
        source: element.dataset.source,
        mediaId: `short-media-${index}`,
        frame: element.querySelector('.short-frame'),
        ambient: element.querySelector('.short-ambient').getContext('2d', { alpha: false }),
        scrub: element.querySelector('.short-scrub'),
        time: element.querySelector('.short-time'),
        stats: element.querySelector('.short-stats'),
        loadingStatus: element.querySelector('.short-loading-status'),
        video: null,
        loading: false,
        userPaused: false,
        gesture: null,
        scrubbing: null,
        seekChainUntil: 0,
        seekTotal: 0,
    }));

    feed = {
        root,
        scroller,
        sheet: root.querySelector('.sheet'),
        items,
        active: -1,
        muted: true,
        volume: prefs.volume,
        rate: prefs.rate,
        fit: prefs.fit,
        lastTap: null,
        tapTimer: null,
        lastAmbient: 0,
        frameId: 0,
        idleTimer: null,
        listeners: [],
        supportsVolume: detectVolumeSupport(),
    };

    feed.observer = new IntersectionObserver(onIntersect, { root: scroller, threshold: VISIBLE_RATIO });
    feed.statsTimer = setInterval(updateStats, STATS_INTERVAL_MS);
    listen(window, 'keydown', onKeyDown);
    listen(document, 'visibilitychange', onVisibilityChange);
    listen(document, 'fullscreenchange', onFullscreenChange);
    listen(document, 'webkitfullscreenchange', onFullscreenChange);
    listen(root, 'pointerdown', wake);
    listen(root, 'pointermove', wake);
    listen(root.querySelector('.feed-prev'), 'click', () => scrollToItem(feed.active - 1));
    listen(root.querySelector('.feed-next'), 'click', () => scrollToItem(feed.active + 1));
    listen(root.querySelector('.feed-theme'), 'click', toggleTheme);
    listen(matchMedia('(prefers-color-scheme: light)'), 'change', onSystemThemeChange);

    for (const item of items) {
        const gesture = item.element.querySelector('.short-gesture');
        listen(gesture, 'pointerdown', (e) => onGestureDown(item, e));
        listen(gesture, 'pointermove', (e) => onGestureMove(item, e));
        listen(gesture, 'pointerup', (e) => onGestureUp(item, e));
        listen(gesture, 'pointercancel', () => cancelGesture(item));
        listen(gesture, 'contextmenu', (e) => e.preventDefault());

        listen(item.scrub, 'pointerdown', (e) => onScrubDown(item, e));
        listen(item.scrub, 'pointermove', (e) => onScrubMove(item, e));
        listen(item.scrub, 'pointerup', (e) => onScrubUp(item, e));
        listen(item.scrub, 'pointercancel', () => endScrub(item, false));

        listen(item.element.querySelector('.short-sound'), 'click', () => setMuted(!feed.muted));
        listen(item.element.querySelector('.short-settings'), 'click', openSheet);
        listen(item.element.querySelector('.short-fullscreen'), 'click', toggleFullscreen);
        listen(item.element.querySelector('.short-unmute'), 'click', () => setMuted(false));
        listen(item.element.querySelector('.short-retry'), 'click', () => retry(item));
        listen(item.element.querySelector('.short-description'), 'click', (e) => e.currentTarget.classList.toggle('is-expanded'));
        feed.observer.observe(item.element);
    }

    initSheet();
    root.classList.toggle('no-volume', !feed.supportsVolume);
    applyFit();
    applyRate();
    applyAudio();
    feed.frameId = requestAnimationFrame(tick);
}

export function disposeShortsFeed() {
    if (!feed) return;
    const current = feed;
    feed = null;
    current.observer.disconnect();
    clearInterval(current.statsTimer);
    clearTimeout(current.tapTimer);
    clearTimeout(current.idleTimer);
    if (fullscreenElement() === current.root) exitFullscreen();
    cancelAnimationFrame(current.frameId);
    for (const [target, type, handler] of current.listeners) target.removeEventListener(type, handler);
    current.items.forEach(unload);
}

function listen(target, type, handler) {
    target.addEventListener(type, handler);
    feed.listeners.push([target, type, handler]);
}

// ── Feed: which item plays, which ones are loaded ─────────────

function onIntersect(entries) {
    for (const entry of entries) {
        if (entry.isIntersecting) activate(Number(entry.target.dataset.index));
    }
}

function activate(index) {
    if (!feed || index === feed.active) return;
    feed.active = index;
    closeSheet();
    feed.root.querySelector('.feed-prev').disabled = index === 0;
    feed.root.querySelector('.feed-next').disabled = index === feed.items.length - 1;

    for (const item of feed.items) {
        item.element.classList.toggle('is-active', item.index === index);
        if (Math.abs(item.index - index) > PRELOAD_DISTANCE) {
            unload(item);
            continue;
        }

        // Only the visible item starts a download here; the next one is preloaded once this one plays (preloadNext),
        // so on a slow connection the video being watched never shares bandwidth with the ones around it.
        if (item.index === index) {
            load(item);
            item.userPaused = false;
            play(item);
        } else {
            cancelGesture(item);
            if (item.video) {
                item.video.pause();
                setState(item, 'ready');
            }
        }
    }

    updateStats();
}

async function load(item) {
    if (item.video || item.loading) return;
    item.loading = true;
    setState(item, 'loading');

    try {
        const video = await startDownload(item.source, item.mediaId, {
            autoplay: false,
            controls: false,
            muted: feed?.muted ?? true,
            preload: 'auto',
        });
        item.loading = false;
        if (!feed) return;
        item.video = video;
        video.loop = true;
        video.playsInline = true;
        video.disableRemotePlayback = true;
        video.setAttribute('playsinline', '');
        video.addEventListener('playing', () => {
            setState(item, 'playing');
            if (item.index === feed?.active) preloadNext();
        });
        video.addEventListener('waiting', () => setState(item, 'loading'));
        video.addEventListener('pause', () => item.userPaused && setState(item, 'paused'));
        video.addEventListener('loadeddata', () => drawAmbient(item));
        video.addEventListener('seeked', () => drawAmbient(item));
        applyAudio();
        video.playbackRate = feed.rate;
        drawAmbient(item);

        if (item.index === feed.active) play(item);
        else setState(item, 'ready');
    } catch (error) {
        item.loading = false;
        // Cancellation means the item scrolled out of range, not that it failed.
        if (!feed || /cancelled/i.test(error?.message ?? '')) return;
        console.warn(`Short ${item.index} failed to load:`, error);
        setState(item, 'error');
    }
}

function preloadNext() {
    const next = feed.items[feed.active + 1];
    if (next) load(next);
}

function unload(item) {
    if (!item.video && !item.loading) return;
    stopDownload(item.source);
    item.video?.pause();
    item.video = null;
    item.loading = false;
    item.scrubbing = null;
    document.getElementById(item.mediaId)?.replaceChildren();
    item.frame.classList.remove('has-ambient');
    setProgress(item, 0, 0);
    setState(item, 'idle');
}

function retry(item) {
    unload(item);
    load(item);
}

// ── Playback ──────────────────────────────────────────────────

function play(item) {
    const video = item.video;
    if (!video) return;
    video.play().catch((error) => {
        // Autoplay with sound needs a user gesture: fall back to muted playback instead of a frozen frame.
        if (error?.name === 'NotAllowedError' && !video.muted && feed) {
            setMuted(true);
            video.play().catch(() => setState(item, 'paused'));
        }
    });
}

function togglePlay(item) {
    const video = item.video;
    if (!video) return;
    if (video.paused) {
        item.userPaused = false;
        play(item);
        flash(item, 'play');
    } else {
        item.userPaused = true;
        video.pause();
        flash(item, 'pause');
    }
}

function seekBy(item, seconds) {
    const video = item.video;
    if (!video || !Number.isFinite(video.duration)) return;
    video.currentTime = Math.min(Math.max(video.currentTime + seconds, 0), Math.max(video.duration - 0.25, 0));
    setProgress(item, video.currentTime / video.duration);
}

function flash(item, icon) {
    const element = item.element.querySelector('.short-flash');
    element.dataset.icon = icon;
    restartAnimation(element, 'is-on');
}

function restartAnimation(element, className) {
    element.classList.remove(className);
    void element.offsetWidth;
    element.classList.add(className);
}

// ── Gestures on the video surface ─────────────────────────────

function onGestureDown(item, event) {
    if (event.button > 0 || !item.video) return;
    // In full screen a tap on hidden controls only brings them back.
    const wasIdle = feed.root.classList.contains('is-idle');
    const gesture = { id: event.pointerId, x: event.clientX, y: event.clientY, moved: false, boosting: false, wasIdle };
    gesture.timer = setTimeout(() => startBoost(item, gesture), LONG_PRESS_MS);
    item.gesture = gesture;
}

function onGestureMove(item, event) {
    const gesture = item.gesture;
    if (!gesture || gesture.id !== event.pointerId || gesture.moved) return;
    if (Math.hypot(event.clientX - gesture.x, event.clientY - gesture.y) > TAP_SLOP_PX) {
        gesture.moved = true;
        if (!gesture.boosting) clearTimeout(gesture.timer);
    }
}

function onGestureUp(item, event) {
    const gesture = item.gesture;
    if (!gesture || gesture.id !== event.pointerId) return;
    item.gesture = null;
    clearTimeout(gesture.timer);
    if (gesture.boosting) return endBoost(item);
    if (!gesture.moved && !gesture.wasIdle) onTap(item, event.clientX);
}

function cancelGesture(item) {
    const gesture = item.gesture;
    if (!gesture) return;
    item.gesture = null;
    clearTimeout(gesture.timer);
    if (gesture.boosting) endBoost(item);
}

function onTap(item, x) {
    const rect = item.frame.getBoundingClientRect();
    const zone = x < rect.left + rect.width / 3 ? -1 : x > rect.right - rect.width / 3 ? 1 : 0;
    const now = performance.now();
    const previous = feed.lastTap;
    feed.lastTap = { item, zone, at: now };

    // The center toggles at once; the sides wait briefly to tell a single tap from a double-tap seek.
    if (zone === 0) {
        clearTimeout(feed.tapTimer);
        return togglePlay(item);
    }

    const chained = item.seekChainUntil > now;
    const doubleTap = previous?.item === item && previous.zone === zone && now - previous.at < DOUBLE_TAP_MS;
    if (chained || doubleTap) {
        clearTimeout(feed.tapTimer);
        feed.lastTap = null;
        return seekGesture(item, zone, chained);
    }

    clearTimeout(feed.tapTimer);
    feed.tapTimer = setTimeout(() => togglePlay(item), DOUBLE_TAP_MS);
}

function seekGesture(item, direction, chained) {
    item.seekTotal = chained && Math.sign(item.seekTotal) === direction ? item.seekTotal + direction * SEEK_STEP_S : direction * SEEK_STEP_S;
    item.seekChainUntil = performance.now() + SEEK_CHAIN_MS;
    seekBy(item, direction * SEEK_STEP_S);

    const ripple = item.element.querySelector(`.short-seek[data-direction="${direction}"]`);
    ripple.querySelector('.short-seek-label').textContent = `${Math.abs(item.seekTotal)} s`;
    restartAnimation(ripple, 'is-on');
}

function startBoost(item, gesture) {
    if (!item.video || item.video.paused || gesture.moved) return;
    gesture.boosting = true;
    item.video.playbackRate = BOOST_RATE;
    item.element.classList.add('is-boosting');
    navigator.vibrate?.(8);
}

function endBoost(item) {
    item.element.classList.remove('is-boosting');
    if (item.video && feed) item.video.playbackRate = feed.rate;
}

// ── Seek bar ──────────────────────────────────────────────────

function onScrubDown(item, event) {
    const video = item.video;
    if (event.button > 0 || !video || !Number.isFinite(video.duration)) return;
    event.preventDefault();
    item.scrub.setPointerCapture(event.pointerId);
    item.scrubbing = { id: event.pointerId, ratio: 0 };
    item.element.classList.add('is-scrubbing');
    onScrubMove(item, event);
}

function onScrubMove(item, event) {
    const scrubbing = item.scrubbing;
    if (!scrubbing || scrubbing.id !== event.pointerId) return;
    const rect = item.scrub.getBoundingClientRect();
    scrubbing.ratio = Math.min(Math.max((event.clientX - rect.left) / rect.width, 0), 1);
    setProgress(item, scrubbing.ratio);
    item.time.textContent = `${formatTime(scrubbing.ratio * item.video.duration)} / ${formatTime(item.video.duration)}`;
}

function onScrubUp(item, event) {
    if (item.scrubbing?.id === event.pointerId) endScrub(item, true);
}

function endScrub(item, commit) {
    const scrubbing = item.scrubbing;
    if (!scrubbing) return;
    item.scrubbing = null;
    item.element.classList.remove('is-scrubbing');
    // Seek once on release: every seek on a torrent stream asks peers for new pieces.
    if (commit && item.video) item.video.currentTime = scrubbing.ratio * item.video.duration;
}

function setProgress(item, ratio, buffered) {
    item.scrub.style.setProperty('--progress', ratio.toFixed(4));
    item.scrub.setAttribute('aria-valuenow', Math.round(ratio * 100));
    if (buffered !== undefined) item.scrub.style.setProperty('--buffered', buffered.toFixed(4));
}

// ── Frame loop: progress bar and ambient backdrop of the active item ──

function tick(now) {
    if (!feed) return;
    feed.frameId = requestAnimationFrame(tick);

    const item = feed.items[feed.active];
    const video = item?.video;
    if (!video || item.scrubbing || !video.duration) return;

    setProgress(item, video.currentTime / video.duration);
    if (item.element.dataset.state === 'paused') item.time.textContent = `${formatTime(video.currentTime)} / ${formatTime(video.duration)}`;
    if (!video.paused && now - feed.lastAmbient > AMBIENT_INTERVAL_MS) {
        feed.lastAmbient = now;
        drawAmbient(item);
    }
}

// A tiny canvas of the current frame, scaled up and blurred by CSS, fills the space around a letterboxed video.
function drawAmbient(item) {
    const video = item.video;
    if (!video || video.readyState < 2) return;
    try {
        item.ambient.drawImage(video, 0, 0, item.ambient.canvas.width, item.ambient.canvas.height);
        item.frame.classList.add('has-ambient');
    } catch {
        // A frame that is not decodable yet; the next tick tries again.
    }
}

// ── Audio, speed and framing ──────────────────────────────────

function setMuted(muted) {
    if (!feed) return;
    feed.muted = muted;
    if (!muted) {
        feed.root.classList.add('hint-dismissed');
        if (feed.volume === 0) feed.volume = 1;
    }
    applyAudio();
    savePrefs();
}

function setVolume(volume) {
    if (!feed) return;
    feed.volume = volume;
    feed.muted = volume === 0;
    if (!feed.muted) feed.root.classList.add('hint-dismissed');
    applyAudio();
    savePrefs();
}

function applyAudio() {
    for (const item of feed.items) {
        if (!item.video) continue;
        item.video.muted = feed.muted;
        item.video.volume = feed.volume;
    }
    feed.root.classList.toggle('is-muted', feed.muted);
    const range = feed.sheet.querySelector('.sheet-range');
    range.value = feed.muted ? 0 : feed.volume;
    range.style.setProperty('--value', range.value);
    for (const button of feed.root.querySelectorAll('.short-sound, .sheet-mute')) {
        button.setAttribute('aria-label', feed.muted ? 'Ativar som' : 'Desativar som');
    }
}

function setRate(rate) {
    if (!feed) return;
    feed.rate = rate;
    applyRate();
    savePrefs();
}

function stepRate(direction) {
    const index = RATES.indexOf(feed.rate);
    const next = RATES[Math.min(Math.max((index < 0 ? RATES.indexOf(1) : index) + direction, 0), RATES.length - 1)];
    setRate(next);
}

function applyRate() {
    for (const item of feed.items) {
        if (item.video && !item.element.classList.contains('is-boosting')) item.video.playbackRate = feed.rate;
    }
    const label = `${feed.rate}×`;
    for (const element of feed.root.querySelectorAll('.short-rate-label')) element.textContent = label;
    feed.root.classList.toggle('has-rate', feed.rate !== 1);
    for (const option of feed.sheet.querySelectorAll('[data-rate]')) {
        option.classList.toggle('is-on', Number(option.dataset.rate) === feed.rate);
    }
}

function applyFit() {
    feed.root.classList.toggle('fit-cover', feed.fit === 'cover');
    for (const option of feed.sheet.querySelectorAll('[data-fit]')) option.classList.toggle('is-on', option.dataset.fit === feed.fit);
}

// ── Settings sheet ────────────────────────────────────────────

function initSheet() {
    const sheet = feed.sheet;
    listen(sheet.querySelector('.sheet-backdrop'), 'click', closeSheet);
    listen(sheet.querySelector('.sheet-mute'), 'click', () => setMuted(!feed.muted));
    listen(sheet.querySelector('.sheet-range'), 'input', (e) => setVolume(Number(e.currentTarget.value)));
    for (const option of sheet.querySelectorAll('[data-rate]')) {
        listen(option, 'click', () => setRate(Number(option.dataset.rate)));
    }
    for (const option of sheet.querySelectorAll('[data-fit]')) {
        listen(option, 'click', () => {
            feed.fit = option.dataset.fit;
            applyFit();
            savePrefs();
        });
    }
}

function openSheet() {
    feed.root.classList.add('is-sheet-open');
    feed.sheet.setAttribute('aria-hidden', 'false');
}

function closeSheet() {
    if (!feed?.root.classList.contains('is-sheet-open')) return;
    feed.root.classList.remove('is-sheet-open');
    feed.sheet.setAttribute('aria-hidden', 'true');
}

// ── Full screen ───────────────────────────────────────────────

// The whole feed goes full screen, so swiping and the up/down buttons keep working inside it.
async function toggleFullscreen() {
    if (!feed) return;
    if (fullscreenElement()) return exitFullscreen();

    const root = feed.root;
    const request = root.requestFullscreen ?? root.webkitRequestFullscreen;
    const video = feed.items[feed.active]?.video;
    if (!request) {
        // iPhone Safari has no element full screen; its native video player is the only option.
        video?.webkitEnterFullscreen?.();
        return;
    }

    try {
        await request.call(root, { navigationUI: 'hide' });
        // Landscape movies fill a phone only when it is turned sideways.
        if (video && video.videoWidth > video.videoHeight) await screen.orientation?.lock?.('landscape');
    } catch {
        // Orientation lock is unsupported on desktop and some browsers; full screen still applies.
    }
}

function exitFullscreen() {
    (document.exitFullscreen ?? document.webkitExitFullscreen)?.call(document)?.catch?.(() => {});
}

function fullscreenElement() {
    return document.fullscreenElement ?? document.webkitFullscreenElement ?? null;
}

function onFullscreenChange() {
    if (!feed) return;
    const active = fullscreenElement() === feed.root;
    feed.root.classList.toggle('is-fullscreen', active);
    for (const button of feed.root.querySelectorAll('.short-fullscreen')) {
        button.setAttribute('aria-label', active ? 'Sair da tela cheia' : 'Tela cheia');
    }
    if (!active) {
        try { screen.orientation?.unlock?.(); } catch { /* not supported */ }
    }
    wake();
    // The viewport just changed size: keep the current video snapped in place.
    requestAnimationFrame(() => feed?.items[feed.active]?.element.scrollIntoView({ block: 'start' }));
}

// In full screen the controls fade out after a moment without touches, mouse or keys.
function wake() {
    if (!feed) return;
    feed.root.classList.remove('is-idle');
    clearTimeout(feed.idleTimer);
    if (!feed.root.classList.contains('is-fullscreen')) return;
    feed.idleTimer = setTimeout(() => {
        const video = feed?.items[feed.active]?.video;
        if (video && !video.paused && !feed.root.classList.contains('is-sheet-open')) feed.root.classList.add('is-idle');
    }, IDLE_MS);
}

// ── Keyboard and page visibility ──────────────────────────────

function onKeyDown(event) {
    if (!feed || event.target.closest?.('input, textarea') || event.ctrlKey || event.metaKey || event.altKey) return;
    const current = feed.items[feed.active];
    if (feed.root.classList.contains('is-sheet-open')) {
        if (event.key === 'Escape') {
            closeSheet();
            event.preventDefault();
        }
        return;
    }

    wake();
    switch (event.key) {
        case 'ArrowDown':
        case 'j':
            scrollToItem(feed.active + 1);
            break;
        case 'ArrowUp':
        case 'k':
            scrollToItem(feed.active - 1);
            break;
        case 'ArrowLeft':
            if (current) seekBy(current, -KEY_SEEK_STEP_S);
            break;
        case 'ArrowRight':
            if (current) seekBy(current, KEY_SEEK_STEP_S);
            break;
        case ' ':
            if (current) togglePlay(current);
            break;
        case 'm':
            setMuted(!feed.muted);
            break;
        case 'f':
            toggleFullscreen();
            break;
        case '<':
            stepRate(-1);
            break;
        case '>':
            stepRate(1);
            break;
        default:
            return;
    }
    event.preventDefault();
}

function onVisibilityChange() {
    const item = feed?.items[feed.active];
    if (!item?.video) return;
    if (document.hidden) item.video.pause();
    else if (!item.userPaused) play(item);
}

function scrollToItem(index) {
    feed?.items[index]?.element.scrollIntoView({ behavior: 'smooth' });
}

// ── Status text ───────────────────────────────────────────────

function setState(item, state) {
    item.element.dataset.state = state;
}

function updateStats() {
    if (!feed) return;
    for (const item of feed.items) {
        const stats = item.index === feed.active ? getTorrentStats(item.source) : null;
        item.stats.textContent = stats ? formatStats(stats) : '';
        item.loadingStatus.textContent = item.index === feed.active ? formatLoading(stats) : '';
        if (item.index === feed.active && item.video?.duration) setProgress(item, item.video.currentTime / item.video.duration, bufferedRatio(item.video));
    }
}

function bufferedRatio(video) {
    const ranges = video.buffered;
    for (let i = 0; i < ranges.length; i++) {
        if (ranges.start(i) <= video.currentTime && video.currentTime <= ranges.end(i)) return ranges.end(i) / video.duration;
    }
    return video.currentTime / video.duration;
}

function formatStats(stats) {
    if (stats.progress >= 100) return `P2P · ${formatPeers(stats.peers)} · completo`;
    return `P2P · ${formatPeers(stats.peers)} · ${formatSpeed(stats.downloadSpeed)} · ${Math.floor(stats.progress)}%`;
}

// What the loading overlay says while a video gets ready: which step it is on, so a slow start never looks frozen.
function formatLoading(stats) {
    if (!stats) return 'Conectando…';
    if (!stats.ready) return stats.peers ? `Lendo o torrent · ${formatPeers(stats.peers)}` : 'Procurando pares…';
    return `Carregando · ${formatPeers(stats.peers)} · ${formatSpeed(stats.downloadSpeed)}`;
}

function formatPeers(count) {
    return `${count} ${count === 1 ? 'peer' : 'peers'}`;
}

function formatSpeed(bytesPerSecond) {
    return bytesPerSecond >= 1024 * 1024
        ? `${(bytesPerSecond / (1024 * 1024)).toFixed(1)} MB/s`
        : `${Math.round(bytesPerSecond / 1024)} KB/s`;
}

function formatTime(seconds) {
    if (!Number.isFinite(seconds)) return '0:00';
    const total = Math.floor(seconds);
    const minutes = Math.floor(total / 60);
    const rest = String(total % 60).padStart(2, '0');
    return minutes >= 60 ? `${Math.floor(minutes / 60)}:${String(minutes % 60).padStart(2, '0')}:${rest}` : `${minutes}:${rest}`;
}

// ── Theme: DS dark or light (parchment); index.html applies it before the first paint ──

function toggleTheme() {
    const next = document.documentElement.dataset.theme === 'light' ? 'dark' : 'light';
    applyTheme(next);
    try {
        localStorage.setItem(THEME_KEY, next);
    } catch {
        // Without storage the choice lasts for this visit only.
    }
}

// The system preference applies only until the viewer picks a theme.
function onSystemThemeChange(e) {
    let stored = null;
    try {
        stored = localStorage.getItem(THEME_KEY);
    } catch {
        // Treat unavailable storage as no saved choice.
    }
    if (!stored) applyTheme(e.matches ? 'light' : 'dark');
}

function applyTheme(theme) {
    document.documentElement.dataset.theme = theme;
    document.querySelector('meta[name="theme-color"]')?.setAttribute('content', THEME_COLORS[theme]);
}

// ── Preferences ───────────────────────────────────────────────

function loadPrefs() {
    const defaults = { volume: 1, rate: 1, fit: 'cover' };
    try {
        const stored = JSON.parse(localStorage.getItem(PREFS_KEY) ?? '{}');
        return {
            volume: Number.isFinite(stored.volume) ? Math.min(Math.max(stored.volume, 0), 1) : defaults.volume,
            rate: RATES.includes(stored.rate) ? stored.rate : defaults.rate,
            fit: stored.fit === 'contain' ? 'contain' : defaults.fit,
        };
    } catch {
        return defaults;
    }
}

function savePrefs() {
    try {
        localStorage.setItem(PREFS_KEY, JSON.stringify({ volume: feed.volume, rate: feed.rate, fit: feed.fit }));
    } catch {
        // Storage can be unavailable (private mode); preferences then last for this visit only.
    }
}

// iOS ignores video.volume (only the hardware buttons change it), so the slider is hidden there.
function detectVolumeSupport() {
    const probe = document.createElement('video');
    probe.volume = 0.5;
    return probe.volume === 0.5;
}

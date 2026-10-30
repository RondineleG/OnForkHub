// Vertical shorts feed: one snap-scrolled item per torrent. Only the visible item plays; its neighbours are
// preloaded and anything further away is released so bandwidth goes to what the viewer is about to watch.
import { startDownload, stopDownload, getTorrentStats } from './webtorrentService.js';

const PRELOAD_DISTANCE = 1;
const VISIBLE_RATIO = 0.6;
const STATS_INTERVAL_MS = 1000;

let feed = null;

export function initShortsFeed(feedElement) {
    disposeShortsFeed();

    const items = [...feedElement.querySelectorAll('.short')].map((element, index) => ({
        element,
        index,
        source: element.dataset.source,
        mediaId: `short-media-${index}`,
        video: null,
        loading: false,
        userPaused: false,
    }));

    feed = { element: feedElement, items, active: -1, muted: true };
    feed.observer = new IntersectionObserver(onIntersect, { root: feedElement, threshold: VISIBLE_RATIO });
    feed.statsTimer = setInterval(updateStats, STATS_INTERVAL_MS);
    feed.onKeyDown = onKeyDown;
    window.addEventListener('keydown', feed.onKeyDown);

    for (const item of items) {
        item.element.querySelector('.short-media').addEventListener('click', () => togglePlay(item));
        item.element.querySelector('.short-sound').addEventListener('click', toggleMute);
        item.element.querySelector('.short-retry').addEventListener('click', () => retry(item));
        feed.observer.observe(item.element);
    }

    updateSoundButtons();
}

export function disposeShortsFeed() {
    if (!feed) return;
    const current = feed;
    feed = null;
    current.observer.disconnect();
    clearInterval(current.statsTimer);
    window.removeEventListener('keydown', current.onKeyDown);
    current.items.forEach(unload);
}

function onIntersect(entries) {
    for (const entry of entries) {
        if (entry.isIntersecting) activate(Number(entry.target.dataset.index));
    }
}

function activate(index) {
    if (!feed || index === feed.active) return;
    feed.active = index;

    for (const item of feed.items) {
        const distance = Math.abs(item.index - index);
        if (distance > PRELOAD_DISTANCE) {
            unload(item);
            continue;
        }

        load(item);
        if (item.index === index) {
            item.userPaused = false;
            play(item);
        } else {
            item.video?.pause();
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
        item.video = video;
        video.muted = feed?.muted ?? true;
        video.loop = true;
        video.playsInline = true;
        video.setAttribute('playsinline', '');
        video.addEventListener('timeupdate', () => updateProgress(item));
        video.addEventListener('playing', () => setState(item, 'playing'));
        video.addEventListener('waiting', () => setState(item, 'loading'));
        video.addEventListener('pause', () => item.userPaused && setState(item, 'paused'));

        if (feed && item.index === feed.active) play(item);
        else setState(item, 'ready');
    } catch (error) {
        item.loading = false;
        // Cancellation means the item scrolled out of range, not that it failed.
        if (!feed || /cancelled/i.test(error?.message ?? '')) return;
        console.warn(`Short ${item.index} failed to load:`, error);
        setState(item, 'error');
    }
}

function unload(item) {
    if (!item.video && !item.loading) return;
    stopDownload(item.source);
    item.video?.pause();
    item.video = null;
    item.loading = false;
    document.getElementById(item.mediaId)?.replaceChildren();
    updateProgress(item);
    setState(item, 'idle');
}

function retry(item) {
    unload(item);
    load(item);
}

function play(item) {
    const video = item.video;
    if (!video) return;
    video.play().catch((error) => {
        // Autoplay with sound needs a user gesture: fall back to muted playback instead of a frozen frame.
        if (error?.name === 'NotAllowedError' && !video.muted && feed) {
            feed.muted = true;
            updateSoundButtons();
            video.muted = true;
            video.play().catch(() => setState(item, 'paused'));
        }
    });
}

function togglePlay(item) {
    if (!item.video) return;
    if (item.video.paused) {
        item.userPaused = false;
        play(item);
    } else {
        item.userPaused = true;
        item.video.pause();
    }
}

function toggleMute() {
    if (!feed) return;
    feed.muted = !feed.muted;
    for (const item of feed.items) {
        if (item.video) item.video.muted = feed.muted;
    }
    updateSoundButtons();
}

function onKeyDown(event) {
    if (!feed || event.target.closest?.('input, textarea')) return;
    const current = feed.items[feed.active];
    switch (event.key) {
        case 'ArrowDown':
        case 'j':
            scrollToItem(feed.active + 1);
            break;
        case 'ArrowUp':
        case 'k':
            scrollToItem(feed.active - 1);
            break;
        case ' ':
            if (current) togglePlay(current);
            break;
        case 'm':
            toggleMute();
            break;
        default:
            return;
    }
    event.preventDefault();
}

function scrollToItem(index) {
    feed?.items[index]?.element.scrollIntoView({ behavior: 'smooth' });
}

function setState(item, state) {
    item.element.dataset.state = state;
}

function updateProgress(item) {
    const fill = item.element.querySelector('.short-progress-fill');
    const video = item.video;
    const ratio = video && video.duration ? video.currentTime / video.duration : 0;
    fill.style.width = `${(ratio * 100).toFixed(2)}%`;
}

function updateSoundButtons() {
    if (!feed) return;
    for (const item of feed.items) {
        const button = item.element.querySelector('.short-sound');
        button.classList.toggle('is-muted', feed.muted);
        button.setAttribute('aria-label', feed.muted ? 'Ativar som' : 'Desativar som');
    }
}

function updateStats() {
    if (!feed) return;
    for (const item of feed.items) {
        const label = item.element.querySelector('.short-stats');
        const stats = item.index === feed.active ? getTorrentStats(item.source) : null;
        label.textContent = stats ? formatStats(stats) : '';
    }
}

function formatStats(stats) {
    const peers = `${stats.peers} ${stats.peers === 1 ? 'peer' : 'peers'}`;
    if (stats.progress >= 100) return `P2P · ${peers} · completo`;
    const speed = stats.downloadSpeed >= 1024 * 1024
        ? `${(stats.downloadSpeed / (1024 * 1024)).toFixed(1)} MB/s`
        : `${Math.round(stats.downloadSpeed / 1024)} KB/s`;
    return `P2P · ${peers} · ${speed} · ${Math.floor(stats.progress)}%`;
}

export function playVideo(fileName, videoElement) {
    if (videoElement) {
        console.log("Setting video source to:", `/Videos/${fileName}`);
        videoElement.src = `/Videos/${fileName}`;
        videoElement.volume = 0.1;
        videoElement.load();
        return videoElement.play().catch(error => {
            console.error("Error playing video:", error);
        });
    } else {
        console.error("Video element is not defined.");
    }
}


let touchstartX = 0;
let touchendX = 0;
let isVideoContainer = false;

document.addEventListener('touchstart', e => {
    const target = e.target.closest('.video-container');
    isVideoContainer = !!target;
    if (isVideoContainer) {
        touchstartX = e.changedTouches[0].screenX;
    }
});

document.addEventListener('touchend', e => {
    if (isVideoContainer) {
        touchendX = e.changedTouches[0].screenX;
        handleSwipe();
    }
});

function handleSwipe() {
    const swipeThreshold = 50;
    if (touchendX < touchstartX - swipeThreshold) {
        document.querySelector('.next-button')?.click();
    }
    if (touchendX > touchstartX + swipeThreshold) {
        document.querySelector('.prev-button')?.click();
    }
}
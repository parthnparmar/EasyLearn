// Course System JavaScript - Modern UI Interactions

class CourseSystem {
    constructor() {
        this.currentLesson = null;
        this.progress = {};
        this.init();
    }

    init() {
        this.initVideoPlayer();
        this.initLessonPlaylist();
        this.initProgressTracking();
        this.initAnimations();
        this.initFilters();
    }

    // Video Player Functionality
    initVideoPlayer() {
        const videoContainer = document.querySelector('.video-player-container');
        if (!videoContainer) return;

        // YouTube API integration
        if (typeof YT !== 'undefined') {
            this.initYouTubePlayer();
        } else {
            // Load YouTube API
            const tag = document.createElement('script');
            tag.src = 'https://www.youtube.com/iframe_api';
            const firstScriptTag = document.getElementsByTagName('script')[0];
            firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);
            
            window.onYouTubeIframeAPIReady = () => {
                this.initYouTubePlayer();
            };
        }

        // Video controls
        this.initVideoControls();
    }

    initYouTubePlayer() {
        const videoFrame = document.querySelector('.video-wrapper iframe');
        if (!videoFrame) return;

        const videoId = this.extractYouTubeId(videoFrame.src);
        if (!videoId) return;

        this.player = new YT.Player(videoFrame, {
            videoId: videoId,
            events: {
                'onReady': this.onPlayerReady.bind(this),
                'onStateChange': this.onPlayerStateChange.bind(this)
            }
        });
    }

    extractYouTubeId(url) {
        const regExp = /^.*(youtu.be\/|v\/|u\/\w\/|embed\/|watch\?v=|&v=)([^#&?]*).*/;
        const match = url.match(regExp);
        return (match && match[2].length === 11) ? match[2] : null;
    }

    onPlayerReady(event) {
        console.log('YouTube player ready');
        this.updateProgress();
    }

    onPlayerStateChange(event) {
        if (event.data === YT.PlayerState.ENDED) {
            this.markLessonComplete();
            this.playNextLesson();
        } else if (event.data === YT.PlayerState.PLAYING) {
            this.startProgressTracking();
        } else if (event.data === YT.PlayerState.PAUSED) {
            this.pauseProgressTracking();
        }
    }

    initVideoControls() {
        // Previous/Next buttons
        const prevBtn = document.querySelector('.video-btn.prev');
        const nextBtn = document.querySelector('.video-btn.next');

        if (prevBtn) {
            prevBtn.addEventListener('click', () => this.playPreviousLesson());
        }

        if (nextBtn) {
            nextBtn.addEventListener('click', () => this.playNextLesson());
        }

        // Fullscreen toggle
        const fullscreenBtn = document.querySelector('.video-btn.fullscreen');
        if (fullscreenBtn) {
            fullscreenBtn.addEventListener('click', () => this.toggleFullscreen());
        }
    }

    // Lesson Playlist
    initLessonPlaylist() {
        const lessonItems = document.querySelectorAll('.lesson-item');
        
        lessonItems.forEach((item, index) => {
            item.addEventListener('click', () => {
                this.playLesson(index, item);
            });
        });

        // Set first lesson as active if none selected
        if (lessonItems.length > 0 && !document.querySelector('.lesson-item.active')) {
            lessonItems[0].classList.add('active');
            this.currentLesson = 0;
        }
    }

    playLesson(index, element) {
        // Remove active class from all lessons
        document.querySelectorAll('.lesson-item').forEach(item => {
            item.classList.remove('active');
        });

        // Add active class to selected lesson
        element.classList.add('active');
        this.currentLesson = index;

        // Update video player
        const videoUrl = element.dataset.videoUrl;
        const lessonTitle = element.querySelector('.lesson-title').textContent;
        
        if (videoUrl) {
            this.loadVideo(videoUrl, lessonTitle);
        }

        // Smooth scroll to video player
        document.querySelector('.video-player-container').scrollIntoView({
            behavior: 'smooth',
            block: 'start'
        });
    }

    loadVideo(url, title) {
        const videoFrame = document.querySelector('.video-wrapper iframe');
        const videoTitle = document.querySelector('.video-title');
        
        if (videoFrame && url) {
            const videoId = this.extractYouTubeId(url);
            if (videoId) {
                videoFrame.src = `https://www.youtube.com/embed/${videoId}?enablejsapi=1&rel=0`;
            }
        }

        if (videoTitle && title) {
            videoTitle.textContent = title;
        }

        // Reset progress for new video
        this.updateProgressBar(0);
    }

    playNextLesson() {
        const lessonItems = document.querySelectorAll('.lesson-item');
        if (this.currentLesson < lessonItems.length - 1) {
            this.playLesson(this.currentLesson + 1, lessonItems[this.currentLesson + 1]);
        }
    }

    playPreviousLesson() {
        const lessonItems = document.querySelectorAll('.lesson-item');
        if (this.currentLesson > 0) {
            this.playLesson(this.currentLesson - 1, lessonItems[this.currentLesson - 1]);
        }
    }

    // Progress Tracking
    initProgressTracking() {
        this.progressInterval = null;
        this.watchTime = 0;
    }

    startProgressTracking() {
        if (this.progressInterval) return;

        this.progressInterval = setInterval(() => {
            this.watchTime += 1;
            this.updateProgress();
        }, 1000);
    }

    pauseProgressTracking() {
        if (this.progressInterval) {
            clearInterval(this.progressInterval);
            this.progressInterval = null;
        }
    }

    updateProgress() {
        if (!this.player || typeof this.player.getCurrentTime !== 'function') return;

        const currentTime = this.player.getCurrentTime();
        const duration = this.player.getDuration();
        
        if (duration > 0) {
            const progress = (currentTime / duration) * 100;
            this.updateProgressBar(progress);

            // Mark as complete if watched 90%
            if (progress >= 90) {
                this.markLessonComplete();
            }
        }
    }

    updateProgressBar(progress) {
        const progressBar = document.querySelector('.progress-bar-fill');
        const progressText = document.querySelector('.progress-text .current');
        
        if (progressBar) {
            progressBar.style.width = `${progress}%`;
        }

        if (progressText && this.player) {
            const currentTime = this.formatTime(this.player.getCurrentTime() || 0);
            const duration = this.formatTime(this.player.getDuration() || 0);
            progressText.textContent = `${currentTime} / ${duration}`;
        }
    }

    markLessonComplete() {
        const activeLesson = document.querySelector('.lesson-item.active');
        if (activeLesson && !activeLesson.classList.contains('completed')) {
            activeLesson.classList.add('completed');
            
            // Update lesson status icon
            const statusIcon = activeLesson.querySelector('.lesson-status');
            if (statusIcon) {
                statusIcon.innerHTML = '<i class="fas fa-check"></i>';
            }

            // Send completion to server
            this.sendLessonCompletion(activeLesson.dataset.lessonId);
            
            // Show completion animation
            this.showCompletionAnimation();
        }
    }

    sendLessonCompletion(lessonId) {
        if (!lessonId) return;

        fetch(`/student/complete-lesson/${lessonId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            }
        }).catch(error => {
            console.error('Error marking lesson complete:', error);
        });
    }

    showCompletionAnimation() {
        // Create completion notification
        const notification = document.createElement('div');
        notification.className = 'completion-notification';
        notification.innerHTML = `
            <div class="completion-content">
                <i class="fas fa-check-circle"></i>
                <span>Lesson Completed!</span>
            </div>
        `;
        
        document.body.appendChild(notification);
        
        // Animate in
        setTimeout(() => notification.classList.add('show'), 100);
        
        // Remove after 3 seconds
        setTimeout(() => {
            notification.classList.remove('show');
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }

    // Utility Functions
    formatTime(seconds) {
        const minutes = Math.floor(seconds / 60);
        const remainingSeconds = Math.floor(seconds % 60);
        return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`;
    }

    toggleFullscreen() {
        const videoContainer = document.querySelector('.video-wrapper');
        
        if (!document.fullscreenElement) {
            videoContainer.requestFullscreen().catch(err => {
                console.error('Error attempting to enable fullscreen:', err);
            });
        } else {
            document.exitFullscreen();
        }
    }

    // Animations
    initAnimations() {
        // Intersection Observer for scroll animations
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('animate-in');
                }
            });
        }, { threshold: 0.1 });

        // Observe course cards
        document.querySelectorAll('.course-card').forEach(card => {
            observer.observe(card);
        });

        // Observe lesson items
        document.querySelectorAll('.lesson-item').forEach(item => {
            observer.observe(item);
        });
    }

    // Course Filters
    initFilters() {
        const searchInput = document.querySelector('#courseSearch');
        const categoryFilter = document.querySelector('#categoryFilter');
        const priceFilter = document.querySelector('#priceFilter');

        if (searchInput) {
            searchInput.addEventListener('input', this.debounce(() => {
                this.filterCourses();
            }, 300));
        }

        if (categoryFilter) {
            categoryFilter.addEventListener('change', () => {
                this.filterCourses();
            });
        }

        if (priceFilter) {
            priceFilter.addEventListener('change', () => {
                this.filterCourses();
            });
        }
    }

    filterCourses() {
        const searchTerm = document.querySelector('#courseSearch')?.value.toLowerCase() || '';
        const selectedCategory = document.querySelector('#categoryFilter')?.value || '';
        const selectedPrice = document.querySelector('#priceFilter')?.value || '';

        const courseCards = document.querySelectorAll('.course-card');

        courseCards.forEach(card => {
            const title = card.querySelector('.course-card-title')?.textContent.toLowerCase() || '';
            const description = card.querySelector('.course-card-description')?.textContent.toLowerCase() || '';
            const category = card.dataset.category || '';
            const price = parseFloat(card.dataset.price) || 0;

            let show = true;

            // Search filter
            if (searchTerm && !title.includes(searchTerm) && !description.includes(searchTerm)) {
                show = false;
            }

            // Category filter
            if (selectedCategory && category !== selectedCategory) {
                show = false;
            }

            // Price filter
            if (selectedPrice === 'free' && price > 0) {
                show = false;
            } else if (selectedPrice === 'paid' && price === 0) {
                show = false;
            }

            // Show/hide with animation
            if (show) {
                card.style.display = 'block';
                setTimeout(() => card.classList.add('fade-in'), 10);
            } else {
                card.classList.remove('fade-in');
                setTimeout(() => card.style.display = 'none', 300);
            }
        });
    }

    debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }
}

// Course Rating System
class CourseRating {
    constructor() {
        this.initRatingSystem();
    }

    initRatingSystem() {
        const ratingContainers = document.querySelectorAll('.rating-input');
        
        ratingContainers.forEach(container => {
            const stars = container.querySelectorAll('.rating-star');
            const input = container.querySelector('input[type="hidden"]');
            
            stars.forEach((star, index) => {
                star.addEventListener('click', () => {
                    this.setRating(container, index + 1);
                    if (input) input.value = index + 1;
                });
                
                star.addEventListener('mouseenter', () => {
                    this.highlightStars(container, index + 1);
                });
            });
            
            container.addEventListener('mouseleave', () => {
                const currentRating = input ? parseInt(input.value) : 0;
                this.highlightStars(container, currentRating);
            });
        });
    }

    setRating(container, rating) {
        container.dataset.rating = rating;
        this.highlightStars(container, rating);
    }

    highlightStars(container, rating) {
        const stars = container.querySelectorAll('.rating-star');
        
        stars.forEach((star, index) => {
            if (index < rating) {
                star.classList.add('active');
                star.innerHTML = '<i class="fas fa-star"></i>';
            } else {
                star.classList.remove('active');
                star.innerHTML = '<i class="far fa-star"></i>';
            }
        });
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new CourseSystem();
    new CourseRating();
});

// Add completion notification styles
const notificationStyles = `
    .completion-notification {
        position: fixed;
        top: 20px;
        right: 20px;
        background: linear-gradient(135deg, #28a745 0%, #20c997 100%);
        color: white;
        padding: 1rem 1.5rem;
        border-radius: 12px;
        box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
        transform: translateX(100%);
        transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
        z-index: 1000;
    }
    
    .completion-notification.show {
        transform: translateX(0);
    }
    
    .completion-content {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        font-weight: 600;
    }
    
    .completion-content i {
        font-size: 1.2rem;
    }
    
    .animate-in {
        animation: slideUp 0.6s ease-out;
    }
`;

// Inject styles
const styleSheet = document.createElement('style');
styleSheet.textContent = notificationStyles;
document.head.appendChild(styleSheet);
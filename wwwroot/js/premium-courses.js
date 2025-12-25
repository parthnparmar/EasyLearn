// Premium Course System - Minimal JS
class CoursePlayer {
    constructor() {
        this.currentLesson = 0;
        this.player = null;
        this.init();
    }

    init() {
        this.initVideoPlayer();
        this.initPlaylist();
        this.initAnimations();
    }

    initVideoPlayer() {
        // YouTube API
        if (typeof YT === 'undefined') {
            const tag = document.createElement('script');
            tag.src = 'https://www.youtube.com/iframe_api';
            document.head.appendChild(tag);
            window.onYouTubeIframeAPIReady = () => this.setupPlayer();
        } else {
            this.setupPlayer();
        }

        // Controls
        document.querySelectorAll('.video-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                if (btn.classList.contains('prev')) this.previousLesson();
                if (btn.classList.contains('next')) this.nextLesson();
            });
        });
    }

    setupPlayer() {
        const iframe = document.querySelector('.video-wrapper iframe');
        if (!iframe) return;

        const videoId = this.extractVideoId(iframe.src);
        if (videoId) {
            this.player = new YT.Player(iframe, {
                videoId: videoId,
                events: {
                    'onStateChange': (event) => {
                        if (event.data === YT.PlayerState.ENDED) {
                            this.markComplete();
                            this.nextLesson();
                        }
                        if (event.data === YT.PlayerState.PLAYING) {
                            this.startProgress();
                        }
                    }
                }
            });
        }
    }

    extractVideoId(url) {
        const match = url.match(/embed\/([^?]+)/);
        return match ? match[1] : null;
    }

    initPlaylist() {
        document.querySelectorAll('.lesson-item').forEach((item, index) => {
            item.addEventListener('click', () => {
                this.playLesson(index, item);
            });
        });
    }

    playLesson(index, element) {
        // Update active lesson
        document.querySelectorAll('.lesson-item').forEach(item => {
            item.classList.remove('active');
        });
        element.classList.add('active');
        this.currentLesson = index;

        // Load video
        const videoUrl = element.dataset.videoUrl;
        if (videoUrl && this.player) {
            const videoId = this.extractVideoId(videoUrl);
            if (videoId) {
                this.player.loadVideoById(videoId);
            }
        }

        // Update title
        const title = element.querySelector('.lesson-title').textContent;
        document.querySelector('.video-header h4').textContent = title;
    }

    nextLesson() {
        const lessons = document.querySelectorAll('.lesson-item');
        if (this.currentLesson < lessons.length - 1) {
            this.playLesson(this.currentLesson + 1, lessons[this.currentLesson + 1]);
        }
    }

    previousLesson() {
        const lessons = document.querySelectorAll('.lesson-item');
        if (this.currentLesson > 0) {
            this.playLesson(this.currentLesson - 1, lessons[this.currentLesson - 1]);
        }
    }

    startProgress() {
        if (this.progressInterval) return;
        
        this.progressInterval = setInterval(() => {
            if (this.player && this.player.getCurrentTime) {
                const current = this.player.getCurrentTime();
                const duration = this.player.getDuration();
                const progress = (current / duration) * 100;
                
                document.querySelector('.progress-fill').style.width = `${progress}%`;
                
                if (progress >= 90) {
                    this.markComplete();
                }
            }
        }, 1000);
    }

    markComplete() {
        const activeLesson = document.querySelector('.lesson-item.active');
        if (activeLesson) {
            activeLesson.classList.add('completed');
            const lessonId = activeLesson.dataset.lessonId;
            
            // Send to server
            fetch(`/student/complete-lesson/${lessonId}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            });
        }
    }

    initAnimations() {
        // Intersection Observer for animations
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('fade-in');
                }
            });
        });

        document.querySelectorAll('.course-card, .lesson-item').forEach(el => {
            observer.observe(el);
        });
    }
}

// Course filtering
class CourseFilter {
    constructor() {
        this.initFilters();
    }

    initFilters() {
        const searchInput = document.querySelector('#courseSearch');
        const categoryFilter = document.querySelector('#categoryFilter');

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
    }

    filterCourses() {
        const search = document.querySelector('#courseSearch')?.value.toLowerCase() || '';
        const category = document.querySelector('#categoryFilter')?.value || '';

        document.querySelectorAll('.course-card').forEach(card => {
            const title = card.querySelector('.course-title')?.textContent.toLowerCase() || '';
            const cardCategory = card.dataset.category || '';

            const matchesSearch = !search || title.includes(search);
            const matchesCategory = !category || cardCategory === category;

            card.style.display = matchesSearch && matchesCategory ? 'block' : 'none';
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

// Initialize on DOM load
document.addEventListener('DOMContentLoaded', () => {
    new CoursePlayer();
    new CourseFilter();
});
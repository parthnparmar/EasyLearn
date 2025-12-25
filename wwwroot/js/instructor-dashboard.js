// Instructor Dashboard JavaScript

document.addEventListener('DOMContentLoaded', function() {
    initializeDashboard();
});

function initializeDashboard() {
    // Initialize counter animations
    animateCounters();
    
    // Initialize file upload
    initializeFileUpload();
    
    // Initialize tooltips
    initializeTooltips();
    
    // Initialize view toggle
    initializeViewToggle();
    
    // Initialize progress bars
    animateProgressBars();
    
    // Initialize notifications
    initializeNotifications();
}

// Counter Animation
function animateCounters() {
    const counters = document.querySelectorAll('.stats-number[data-count]');
    
    counters.forEach(counter => {
        const target = parseInt(counter.getAttribute('data-count'));
        const duration = 2000; // 2 seconds
        const increment = target / (duration / 16); // 60fps
        let current = 0;
        
        const timer = setInterval(() => {
            current += increment;
            if (current >= target) {
                current = target;
                clearInterval(timer);
            }
            counter.textContent = Math.floor(current);
        }, 16);
    });
}

// Progress Bar Animation
function animateProgressBars() {
    const progressBars = document.querySelectorAll('.stats-progress .progress-bar');
    
    // Trigger animation after a delay
    setTimeout(() => {
        progressBars.forEach(bar => {
            const width = bar.style.width;
            bar.style.width = '0%';
            setTimeout(() => {
                bar.style.width = width;
            }, 100);
        });
    }, 500);
}

// File Upload Functionality
function initializeFileUpload() {
    const uploadArea = document.querySelector('.upload-area');
    const fileInput = document.querySelector('.file-input');
    const fileList = document.querySelector('.file-list');
    
    if (!uploadArea || !fileInput) return;
    
    // Click to browse
    uploadArea.addEventListener('click', () => {
        fileInput.click();
    });
    
    // Drag and drop
    uploadArea.addEventListener('dragover', (e) => {
        e.preventDefault();
        uploadArea.style.borderColor = '#667eea';
        uploadArea.style.background = '#f8f9fa';
    });
    
    uploadArea.addEventListener('dragleave', (e) => {
        e.preventDefault();
        uploadArea.style.borderColor = '#dee2e6';
        uploadArea.style.background = 'transparent';
    });
    
    uploadArea.addEventListener('drop', (e) => {
        e.preventDefault();
        uploadArea.style.borderColor = '#dee2e6';
        uploadArea.style.background = 'transparent';
        
        const files = e.dataTransfer.files;
        handleFiles(files);
    });
    
    // File input change
    fileInput.addEventListener('change', (e) => {
        handleFiles(e.target.files);
    });
    
    function handleFiles(files) {
        fileList.innerHTML = '';
        
        Array.from(files).forEach(file => {
            const fileItem = createFileItem(file);
            fileList.appendChild(fileItem);
        });
    }
    
    function createFileItem(file) {
        const item = document.createElement('div');
        item.className = 'file-item d-flex justify-content-between align-items-center p-2 border rounded mb-2';
        
        const fileInfo = document.createElement('div');
        fileInfo.innerHTML = `
            <i class="fas fa-file-pdf text-danger me-2"></i>
            <span>${file.name}</span>
            <small class="text-muted ms-2">(${formatFileSize(file.size)})</small>
        `;
        
        const removeBtn = document.createElement('button');
        removeBtn.className = 'btn btn-sm btn-outline-danger';
        removeBtn.innerHTML = '<i class="fas fa-times"></i>';
        removeBtn.onclick = () => item.remove();
        
        item.appendChild(fileInfo);
        item.appendChild(removeBtn);
        
        return item;
    }
    
    function formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }
}

// View Toggle Functionality
function initializeViewToggle() {
    let isGridView = true;
    
    window.toggleView = function() {
        const container = document.getElementById('coursesContainer');
        const icon = document.getElementById('viewIcon');
        
        if (isGridView) {
            // Switch to list view
            container.style.display = 'flex';
            container.style.flexDirection = 'column';
            container.style.gap = '1rem';
            icon.className = 'fas fa-th-large';
            
            // Modify course items for list view
            const courseItems = container.querySelectorAll('.course-item');
            courseItems.forEach(item => {
                item.style.display = 'flex';
                item.style.flexDirection = 'row';
                item.style.alignItems = 'center';
                
                const thumbnail = item.querySelector('.course-thumbnail');
                if (thumbnail) {
                    thumbnail.style.width = '150px';
                    thumbnail.style.height = '100px';
                    thumbnail.style.flexShrink = '0';
                }
                
                const info = item.querySelector('.course-info');
                if (info) {
                    info.style.flex = '1';
                    info.style.marginLeft = '1rem';
                }
            });
        } else {
            // Switch to grid view
            container.style.display = 'grid';
            container.style.gridTemplateColumns = 'repeat(auto-fill, minmax(300px, 1fr))';
            container.style.gap = '1.5rem';
            icon.className = 'fas fa-list';
            
            // Reset course items for grid view
            const courseItems = container.querySelectorAll('.course-item');
            courseItems.forEach(item => {
                item.style.display = 'block';
                item.style.flexDirection = '';
                item.style.alignItems = '';
                
                const thumbnail = item.querySelector('.course-thumbnail');
                if (thumbnail) {
                    thumbnail.style.width = '';
                    thumbnail.style.height = '150px';
                    thumbnail.style.flexShrink = '';
                }
                
                const info = item.querySelector('.course-info');
                if (info) {
                    info.style.flex = '';
                    info.style.marginLeft = '';
                }
            });
        }
        
        isGridView = !isGridView;
    };
}

// Tooltips
function initializeTooltips() {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// Notifications
function initializeNotifications() {
    window.showNotification = function(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `notification ${type}`;
        notification.innerHTML = `
            <div class="d-flex justify-content-between align-items-center">
                <span>${message}</span>
                <button class="btn-close btn-close-sm ms-2" onclick="this.parentElement.parentElement.remove()"></button>
            </div>
        `;
        
        document.body.appendChild(notification);
        
        // Show notification
        setTimeout(() => {
            notification.classList.add('show');
        }, 100);
        
        // Auto remove after 5 seconds
        setTimeout(() => {
            notification.classList.remove('show');
            setTimeout(() => {
                if (notification.parentElement) {
                    notification.remove();
                }
            }, 300);
        }, 5000);
    };
}

// Analytics Modal
window.showAnalytics = function() {
    const modal = new bootstrap.Modal(document.getElementById('analyticsModal') || createAnalyticsModal());
    modal.show();
};

function createAnalyticsModal() {
    const modal = document.createElement('div');
    modal.className = 'modal fade';
    modal.id = 'analyticsModal';
    modal.tabIndex = -1;
    modal.innerHTML = `
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">
                        <i class="fas fa-chart-line"></i>
                        Course Analytics
                    </h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <div class="row">
                        <div class="col-md-3 mb-3">
                            <div class="text-center">
                                <div class="stats-number text-primary">1,234</div>
                                <div class="stats-label">Total Views</div>
                            </div>
                        </div>
                        <div class="col-md-3 mb-3">
                            <div class="text-center">
                                <div class="stats-number text-success">567</div>
                                <div class="stats-label">Enrollments</div>
                            </div>
                        </div>
                        <div class="col-md-3 mb-3">
                            <div class="text-center">
                                <div class="stats-number text-info">89%</div>
                                <div class="stats-label">Completion Rate</div>
                            </div>
                        </div>
                        <div class="col-md-3 mb-3">
                            <div class="text-center">
                                <div class="stats-number text-warning">4.8</div>
                                <div class="stats-label">Avg Rating</div>
                            </div>
                        </div>
                    </div>
                    
                    <div class="mt-4">
                        <h6>Monthly Revenue</h6>
                        <canvas id="revenueChart" width="400" height="200"></canvas>
                    </div>
                    
                    <div class="mt-4">
                        <h6>Student Engagement</h6>
                        <div class="progress-modern mb-2">
                            <div class="progress-bar-modern" style="width: 85%"></div>
                        </div>
                        <small class="text-muted">85% of students actively participate in discussions</small>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    document.body.appendChild(modal);
    return modal;
}

// Course Management Functions
window.editCourse = function(courseId) {
    window.location.href = `/instructor/edit-course/${courseId}`;
};

window.manageLessons = function(courseId) {
    window.location.href = `/instructor/manage-lessons/${courseId}`;
};

window.manageQuizzes = function(courseId) {
    window.location.href = `/instructor/manage-quizzes/${courseId}`;
};

window.viewStudentPerformance = function(courseId) {
    window.location.href = `/instructor/student-performance/${courseId}`;
};

// Form Submissions
document.addEventListener('submit', function(e) {
    if (e.target.classList.contains('upload-form')) {
        e.preventDefault();
        handleFormSubmission(e.target);
    }
});

function handleFormSubmission(form) {
    const submitBtn = form.querySelector('button[type="submit"]');
    const originalText = submitBtn.innerHTML;
    
    // Show loading state
    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Processing...';
    submitBtn.disabled = true;
    
    // Simulate form submission
    setTimeout(() => {
        showNotification('Content uploaded successfully!', 'success');
        
        // Reset form
        form.reset();
        
        // Reset button
        submitBtn.innerHTML = originalText;
        submitBtn.disabled = false;
        
        // Close modal if it's in a modal
        const modal = form.closest('.modal');
        if (modal) {
            bootstrap.Modal.getInstance(modal).hide();
        }
    }, 2000);
}

// Keyboard Shortcuts
document.addEventListener('keydown', function(e) {
    // Ctrl/Cmd + N for new course
    if ((e.ctrlKey || e.metaKey) && e.key === 'n') {
        e.preventDefault();
        window.location.href = '/instructor/create-course';
    }
    
    // Ctrl/Cmd + U for upload
    if ((e.ctrlKey || e.metaKey) && e.key === 'u') {
        e.preventDefault();
        const uploadModal = document.getElementById('uploadModal');
        if (uploadModal) {
            new bootstrap.Modal(uploadModal).show();
        }
    }
});

// Smooth Scrolling
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});

// Auto-refresh data every 5 minutes
setInterval(() => {
    // Refresh dashboard data
    refreshDashboardData();
}, 300000);

function refreshDashboardData() {
    // This would typically make an AJAX call to get updated data
    console.log('Refreshing dashboard data...');
    
    // Show a subtle indicator that data is being refreshed
    const refreshIndicator = document.createElement('div');
    refreshIndicator.className = 'position-fixed top-0 end-0 m-3 alert alert-info alert-dismissible fade show';
    refreshIndicator.innerHTML = `
        <i class="fas fa-sync-alt fa-spin me-2"></i>
        Refreshing data...
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    document.body.appendChild(refreshIndicator);
    
    // Remove after 3 seconds
    setTimeout(() => {
        if (refreshIndicator.parentElement) {
            refreshIndicator.remove();
        }
    }, 3000);
}

// Performance Monitoring
function trackUserInteraction(action, element) {
    // This would typically send analytics data
    console.log(`User interaction: ${action} on ${element}`);
}

// Add click tracking to important elements
document.addEventListener('click', function(e) {
    if (e.target.matches('.btn-primary, .course-item, .quick-action-item')) {
        trackUserInteraction('click', e.target.className);
    }
});

// Lazy Loading for Images
function initializeLazyLoading() {
    const images = document.querySelectorAll('img[data-src]');
    
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.classList.remove('lazy');
                imageObserver.unobserve(img);
            }
        });
    });
    
    images.forEach(img => imageObserver.observe(img));
}

// Initialize lazy loading if supported
if ('IntersectionObserver' in window) {
    initializeLazyLoading();
}

// Error Handling
window.addEventListener('error', function(e) {
    console.error('Dashboard error:', e.error);
    showNotification('An error occurred. Please refresh the page.', 'error');
});

// Service Worker Registration (for offline support)
if ('serviceWorker' in navigator) {
    navigator.serviceWorker.register('/sw.js')
        .then(registration => {
            console.log('SW registered:', registration);
        })
        .catch(error => {
            console.log('SW registration failed:', error);
        });
}
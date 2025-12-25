// Modern UI JavaScript Enhancements
document.addEventListener('DOMContentLoaded', function() {
    
    // Initialize animations
    initializeAnimations();
    
    // Initialize smooth scrolling
    initializeSmoothScrolling();
    
    // Initialize notifications
    initializeNotifications();
    
    // Initialize loading states
    initializeLoadingStates();
    
    // Initialize tooltips
    initializeTooltips();
});

// Animation initialization
function initializeAnimations() {
    // Add fade-in animation to cards
    const cards = document.querySelectorAll('.card, .modern-card');
    cards.forEach((card, index) => {
        card.style.animationDelay = `${index * 0.1}s`;
        card.classList.add('fade-in');
    });
    
    // Add hover effects to buttons
    const buttons = document.querySelectorAll('.btn');
    buttons.forEach(button => {
        button.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-2px)';
        });
        
        button.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
        });
    });
}

// Smooth scrolling for anchor links
function initializeSmoothScrolling() {
    const links = document.querySelectorAll('a[href^="#"]');
    links.forEach(link => {
        link.addEventListener('click', function(e) {
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
}

// Notification system
function initializeNotifications() {
    // Show TempData messages as notifications
    const tempDataSuccess = document.querySelector('[data-temp-success]');
    const tempDataError = document.querySelector('[data-temp-error]');
    
    if (tempDataSuccess) {
        showNotification(tempDataSuccess.textContent, 'success');
    }
    
    if (tempDataError) {
        showNotification(tempDataError.textContent, 'error');
    }
}

function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `notification ${type}`;
    notification.innerHTML = `
        <div class="d-flex align-items-center">
            <i class="fas fa-${getNotificationIcon(type)} me-2"></i>
            <span>${message}</span>
            <button type="button" class="btn-close ms-auto" onclick="this.parentElement.parentElement.remove()"></button>
        </div>
    `;
    
    document.body.appendChild(notification);
    
    // Show notification
    setTimeout(() => notification.classList.add('show'), 100);
    
    // Auto-hide after 5 seconds
    setTimeout(() => {
        notification.classList.remove('show');
        setTimeout(() => notification.remove(), 300);
    }, 5000);
}

function getNotificationIcon(type) {
    const icons = {
        success: 'check-circle',
        error: 'exclamation-circle',
        warning: 'exclamation-triangle',
        info: 'info-circle'
    };
    return icons[type] || 'info-circle';
}

// Loading states for forms and buttons
function initializeLoadingStates() {
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function() {
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="loading-spinner me-2"></span>Loading...';
            }
        });
    });
}

// Initialize Bootstrap tooltips
function initializeTooltips() {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// Progress bar animation
function animateProgressBar(element, targetWidth) {
    let currentWidth = 0;
    const increment = targetWidth / 50;
    
    const animation = setInterval(() => {
        currentWidth += increment;
        element.style.width = `${currentWidth}%`;
        
        if (currentWidth >= targetWidth) {
            clearInterval(animation);
            element.style.width = `${targetWidth}%`;
        }
    }, 20);
}

// Initialize progress bars on page load
document.addEventListener('DOMContentLoaded', function() {
    const progressBars = document.querySelectorAll('.progress-bar-modern');
    progressBars.forEach(bar => {
        const targetWidth = parseFloat(bar.getAttribute('data-width') || bar.style.width);
        bar.style.width = '0%';
        setTimeout(() => animateProgressBar(bar, targetWidth), 500);
    });
});

// Course card interactions
function initializeCourseCards() {
    const courseCards = document.querySelectorAll('.course-card-modern');
    courseCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-8px) scale(1.02)';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0) scale(1)';
        });
    });
}

// Quiz interactions
function initializeQuizInterface() {
    const answerOptions = document.querySelectorAll('.answer-option');
    answerOptions.forEach(option => {
        option.addEventListener('click', function() {
            // Remove selected class from siblings
            const siblings = this.parentElement.querySelectorAll('.answer-option');
            siblings.forEach(sibling => sibling.classList.remove('selected'));
            
            // Add selected class to clicked option
            this.classList.add('selected');
            
            // Update hidden input if exists
            const hiddenInput = this.parentElement.querySelector('input[type="hidden"]');
            if (hiddenInput) {
                hiddenInput.value = this.getAttribute('data-value');
            }
        });
    });
}

// Table row hover effects
function initializeTableEffects() {
    const tableRows = document.querySelectorAll('.table-modern tbody tr');
    tableRows.forEach(row => {
        row.addEventListener('mouseenter', function() {
            this.style.transform = 'scale(1.01)';
        });
        
        row.addEventListener('mouseleave', function() {
            this.style.transform = 'scale(1)';
        });
    });
}

// Initialize all components when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    initializeCourseCards();
    initializeQuizInterface();
    initializeTableEffects();
});

// Utility function for AJAX requests with loading states
function makeAjaxRequest(url, options = {}) {
    const defaultOptions = {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        }
    };
    
    const finalOptions = { ...defaultOptions, ...options };
    
    return fetch(url, finalOptions)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .catch(error => {
            console.error('AJAX request failed:', error);
            showNotification('An error occurred. Please try again.', 'error');
            throw error;
        });
}

// Export functions for global use
window.EasyLearnUI = {
    showNotification,
    animateProgressBar,
    makeAjaxRequest
};
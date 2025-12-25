// Course Creation Wizard JavaScript

let currentStep = 1;
const totalSteps = 4;

document.addEventListener('DOMContentLoaded', function() {
    initializeWizard();
    initializeFormHandlers();
    initializeFileUploads();
    initializePricing();
    initializePreview();
});

// Initialize Wizard
function initializeWizard() {
    showStep(currentStep);
    updateStepIndicators();
}

// Step Navigation
function changeStep(direction) {
    const newStep = currentStep + direction;
    
    if (newStep < 1 || newStep > totalSteps) return;
    
    // Validate current step before moving forward
    if (direction > 0 && !validateStep(currentStep)) {
        return;
    }
    
    // Hide current step
    document.getElementById(`step${currentStep}`).classList.remove('active');
    
    // Update current step
    currentStep = newStep;
    
    // Show new step
    showStep(currentStep);
    updateStepIndicators();
    updateNavigationButtons();
    
    // Scroll to top
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function showStep(step) {
    document.getElementById(`step${step}`).classList.add('active');
    
    // Update review section if on last step
    if (step === 4) {
        updateReviewSection();
    }
}

function updateStepIndicators() {
    const steps = document.querySelectorAll('.step');
    steps.forEach((step, index) => {
        const stepNumber = index + 1;
        step.classList.remove('active', 'completed');
        
        if (stepNumber === currentStep) {
            step.classList.add('active');
        } else if (stepNumber < currentStep) {
            step.classList.add('completed');
        }
    });
}

function updateNavigationButtons() {
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');
    const submitBtn = document.getElementById('submitBtn');
    
    // Previous button
    prevBtn.style.display = currentStep === 1 ? 'none' : 'inline-block';
    
    // Next/Submit buttons
    if (currentStep === totalSteps) {
        nextBtn.style.display = 'none';
        submitBtn.style.display = 'inline-block';
    } else {
        nextBtn.style.display = 'inline-block';
        submitBtn.style.display = 'none';
    }
}

// Form Validation
function validateStep(step) {
    let isValid = true;
    const stepElement = document.getElementById(`step${step}`);
    
    // Clear previous errors
    stepElement.querySelectorAll('.is-invalid').forEach(el => {
        el.classList.remove('is-invalid');
    });
    
    switch(step) {
        case 1:
            // Validate basic information
            const title = stepElement.querySelector('[name="Title"]');
            const description = stepElement.querySelector('[name="Description"]');
            const category = stepElement.querySelector('[name="CategoryId"]');
            
            if (!title.value.trim()) {
                title.classList.add('is-invalid');
                isValid = false;
            }
            
            if (!description.value.trim()) {
                description.classList.add('is-invalid');
                isValid = false;
            }
            
            if (!category.value) {
                category.classList.add('is-invalid');
                isValid = false;
            }
            break;
            
        case 2:
            // Validate content (at least one lesson)
            const lessons = stepElement.querySelectorAll('.lesson-item');
            if (lessons.length === 0) {
                showNotification('Please add at least one lesson', 'warning');
                isValid = false;
            }
            break;
            
        case 3:
            // Validate pricing
            const priceCards = stepElement.querySelectorAll('.pricing-card');
            const selectedCard = Array.from(priceCards).find(card => card.classList.contains('selected'));
            
            if (!selectedCard) {
                showNotification('Please select a pricing option', 'warning');
                isValid = false;
            }
            break;
            
        case 4:
            // Validate terms agreement
            const agreeTerms = stepElement.querySelector('#agreeTerms');
            if (!agreeTerms.checked) {
                showNotification('Please agree to the terms and conditions', 'warning');
                isValid = false;
            }
            break;
    }
    
    if (!isValid) {
        showNotification('Please fill in all required fields', 'error');
    }
    
    return isValid;
}

// Form Handlers
function initializeFormHandlers() {
    // Character counters
    const titleInput = document.querySelector('[name="Title"]');
    const descriptionInput = document.querySelector('[name="Description"]');
    
    if (titleInput) {
        titleInput.addEventListener('input', function() {
            updateCharCount(this, 200);
            updatePreview();
        });
    }
    
    if (descriptionInput) {
        descriptionInput.addEventListener('input', function() {
            updateCharCount(this, 1000);
            updatePreview();
        });
    }
    
    // Category change
    const categorySelect = document.querySelector('[name="CategoryId"]');
    if (categorySelect) {
        categorySelect.addEventListener('change', updatePreview);
    }
    
    // Form submission
    const form = document.getElementById('courseForm');
    if (form) {
        form.addEventListener('submit', handleFormSubmit);
    }
}

function updateCharCount(input, maxLength) {
    const charCount = input.value.length;
    const counter = input.closest('.form-group').querySelector('.char-count');
    if (counter) {
        counter.textContent = `${charCount}/${maxLength}`;
        
        if (charCount > maxLength * 0.9) {
            counter.style.color = '#dc3545';
        } else if (charCount > maxLength * 0.7) {
            counter.style.color = '#ffc107';
        } else {
            counter.style.color = '#6c757d';
        }
    }
}

// File Uploads
function initializeFileUploads() {
    // Thumbnail upload
    const thumbnailInput = document.getElementById('thumbnailInput');
    if (thumbnailInput) {
        thumbnailInput.addEventListener('change', function(e) {
            handleThumbnailUpload(e.target.files[0]);
        });
    }
    
    // Materials upload
    const materialsInput = document.getElementById('materialsInput');
    if (materialsInput) {
        materialsInput.addEventListener('change', function(e) {
            handleMaterialsUpload(e.target.files);
        });
    }
    
    // Drag and drop for materials
    const uploadZone = document.querySelector('.upload-zone');
    if (uploadZone) {
        uploadZone.addEventListener('dragover', (e) => {
            e.preventDefault();
            uploadZone.style.borderColor = '#667eea';
            uploadZone.style.background = '#f8f9fa';
        });
        
        uploadZone.addEventListener('dragleave', (e) => {
            e.preventDefault();
            uploadZone.style.borderColor = '#dee2e6';
            uploadZone.style.background = 'transparent';
        });
        
        uploadZone.addEventListener('drop', (e) => {
            e.preventDefault();
            uploadZone.style.borderColor = '#dee2e6';
            uploadZone.style.background = 'transparent';
            handleMaterialsUpload(e.dataTransfer.files);
        });
    }
}

function handleThumbnailUpload(file) {
    if (!file) return;
    
    if (!file.type.startsWith('image/')) {
        showNotification('Please upload an image file', 'error');
        return;
    }
    
    const reader = new FileReader();
    reader.onload = function(e) {
        const preview = document.getElementById('thumbnailPreview');
        preview.innerHTML = `<img src="${e.target.result}" alt="Thumbnail" style="width: 100%; height: 100%; object-fit: cover;">`;
        
        // Update hidden input
        document.getElementById('imageUrlInput').value = e.target.result;
        
        // Update preview card
        const previewImage = document.querySelector('.preview-image');
        if (previewImage) {
            previewImage.style.backgroundImage = `url(${e.target.result})`;
            previewImage.style.backgroundSize = 'cover';
            previewImage.innerHTML = '';
        }
    };
    reader.readAsDataURL(file);
}

function handleMaterialsUpload(files) {
    const materialsList = document.querySelector('.materials-list');
    if (!materialsList) return;
    
    Array.from(files).forEach(file => {
        const item = createMaterialItem(file);
        materialsList.appendChild(item);
    });
}

function createMaterialItem(file) {
    const item = document.createElement('div');
    item.className = 'material-item d-flex justify-content-between align-items-center p-3 border rounded mb-2';
    item.style.animation = 'fadeInUp 0.3s ease-out';
    
    const icon = getFileIcon(file.name);
    
    item.innerHTML = `
        <div class="d-flex align-items-center gap-2">
            <i class="${icon}"></i>
            <div>
                <div class="fw-bold">${file.name}</div>
                <small class="text-muted">${formatFileSize(file.size)}</small>
            </div>
        </div>
        <button type="button" class="btn btn-sm btn-outline-danger" onclick="this.parentElement.remove()">
            <i class="fas fa-times"></i>
        </button>
    `;
    
    return item;
}

function getFileIcon(filename) {
    const ext = filename.split('.').pop().toLowerCase();
    const icons = {
        'pdf': 'fas fa-file-pdf text-danger',
        'doc': 'fas fa-file-word text-primary',
        'docx': 'fas fa-file-word text-primary',
        'ppt': 'fas fa-file-powerpoint text-warning',
        'pptx': 'fas fa-file-powerpoint text-warning',
        'xls': 'fas fa-file-excel text-success',
        'xlsx': 'fas fa-file-excel text-success'
    };
    return icons[ext] || 'fas fa-file text-secondary';
}

function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

// Pricing
function initializePricing() {
    const pricingCards = document.querySelectorAll('.pricing-card');
    const priceInput = document.querySelector('[name="Price"]');
    
    pricingCards.forEach(card => {
        card.addEventListener('click', function() {
            pricingCards.forEach(c => c.classList.remove('selected'));
            this.classList.add('selected');
            
            const priceType = this.getAttribute('data-price');
            if (priceType === 'free') {
                if (priceInput) priceInput.value = '0';
                updatePricingPreview(0);
            } else {
                if (priceInput) priceInput.focus();
            }
        });
    });
    
    if (priceInput) {
        priceInput.addEventListener('input', function() {
            const price = parseFloat(this.value) || 0;
            updatePricingPreview(price);
        });
    }
}

function updatePricingPreview(price) {
    const priceAmount = document.querySelector('.price-amount');
    const platformFee = document.querySelector('.breakdown-item:nth-child(1) span:last-child');
    const earnings = document.querySelector('.earnings-amount');
    
    if (price === 0) {
        if (priceAmount) priceAmount.textContent = 'Free';
        if (platformFee) platformFee.textContent = '$0.00';
        if (earnings) earnings.textContent = '$0.00';
    } else {
        const fee = price * 0.1;
        const earning = price - fee;
        
        if (priceAmount) priceAmount.textContent = `$${price.toFixed(2)}`;
        if (platformFee) platformFee.textContent = `$${fee.toFixed(2)}`;
        if (earnings) earnings.textContent = `$${earning.toFixed(2)}`;
    }
    
    // Update preview card
    const previewPrice = document.querySelector('.preview-price');
    if (previewPrice) {
        previewPrice.textContent = price === 0 ? 'Free' : `$${price.toFixed(2)}`;
    }
}

// Preview Updates
function initializePreview() {
    updatePreview();
}

function updatePreview() {
    const title = document.querySelector('[name="Title"]')?.value || 'Your Course Title';
    const description = document.querySelector('[name="Description"]')?.value || 'Course description will appear here...';
    const categorySelect = document.querySelector('[name="CategoryId"]');
    const category = categorySelect?.options[categorySelect.selectedIndex]?.text || 'Category';
    
    // Update preview card
    const previewTitle = document.querySelector('.preview-title');
    const previewDescription = document.querySelector('.preview-description');
    const previewCategory = document.querySelector('.preview-category');
    
    if (previewTitle) previewTitle.textContent = title;
    if (previewDescription) previewDescription.textContent = description.substring(0, 100) + (description.length > 100 ? '...' : '');
    if (previewCategory) previewCategory.textContent = category;
}

// Review Section
function updateReviewSection() {
    // Basic Information
    const title = document.querySelector('[name="Title"]')?.value || '-';
    const categorySelect = document.querySelector('[name="CategoryId"]');
    const category = categorySelect?.options[categorySelect.selectedIndex]?.text || '-';
    const description = document.querySelector('[name="Description"]')?.value || '-';
    
    document.getElementById('reviewTitle').textContent = title;
    document.getElementById('reviewCategory').textContent = category;
    document.getElementById('reviewDescription').textContent = description;
    
    // Content Summary
    const lessonsCount = document.querySelectorAll('.lesson-item').length;
    const materialsCount = document.querySelectorAll('.material-item').length;
    const quizzesCount = document.querySelectorAll('.quiz-item').length;
    
    document.getElementById('reviewLessons').textContent = lessonsCount;
    document.getElementById('reviewMaterials').textContent = materialsCount;
    document.getElementById('reviewQuizzes').textContent = quizzesCount;
    
    // Pricing
    const price = parseFloat(document.querySelector('[name="Price"]')?.value) || 0;
    document.getElementById('reviewPrice').textContent = price === 0 ? 'Free' : `$${price.toFixed(2)}`;
}

// Dynamic Content
window.addObjective = function() {
    const container = document.querySelector('.objectives-container');
    const item = document.createElement('div');
    item.className = 'objective-item';
    item.innerHTML = `
        <input type="text" class="form-control form-control-modern" 
               placeholder="What will students learn?">
        <button type="button" class="btn btn-outline-danger btn-sm remove-objective" onclick="this.parentElement.remove()">
            <i class="fas fa-times"></i>
        </button>
    `;
    container.appendChild(item);
};

window.addLesson = function() {
    const container = document.querySelector('.lessons-container');
    const lessonNumber = container.querySelectorAll('.lesson-item').length + 1;
    
    const item = document.createElement('div');
    item.className = 'lesson-item';
    item.style.animation = 'fadeInUp 0.3s ease-out';
    item.innerHTML = `
        <div class="lesson-header">
            <span class="lesson-number">${lessonNumber}</span>
            <input type="text" class="form-control lesson-title" placeholder="Lesson Title">
            <button type="button" class="btn btn-outline-danger btn-sm" onclick="this.closest('.lesson-item').remove()">
                <i class="fas fa-trash"></i>
            </button>
        </div>
        <div class="lesson-content">
            <div class="row">
                <div class="col-md-6">
                    <label class="form-label">YouTube Video URL</label>
                    <input type="url" class="form-control" placeholder="https://www.youtube.com/watch?v=...">
                </div>
                <div class="col-md-6">
                    <label class="form-label">Duration (minutes)</label>
                    <input type="number" class="form-control" placeholder="15" min="1">
                </div>
            </div>
            <div class="mt-3">
                <label class="form-label">Lesson Description</label>
                <textarea class="form-control" rows="3" placeholder="What will students learn in this lesson?"></textarea>
            </div>
        </div>
    `;
    container.appendChild(item);
};

// Form Submission
function handleFormSubmit(e) {
    e.preventDefault();
    
    if (!validateStep(4)) {
        return;
    }
    
    const submitBtn = document.getElementById('submitBtn');
    const originalText = submitBtn.innerHTML;
    
    // Show loading state
    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Publishing...';
    submitBtn.disabled = true;
    
    // Simulate form submission
    setTimeout(() => {
        showNotification('Course published successfully!', 'success');
        
        // Redirect after 2 seconds
        setTimeout(() => {
            window.location.href = '/instructor/my-courses';
        }, 2000);
    }, 2000);
}

// Save Draft
window.saveDraft = function() {
    showNotification('Draft saved successfully!', 'success');
};

// Notifications
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
    notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    document.body.appendChild(notification);
    
    setTimeout(() => {
        notification.remove();
    }, 5000);
}

// Keyboard Shortcuts
document.addEventListener('keydown', function(e) {
    // Alt + Right Arrow - Next step
    if (e.altKey && e.key === 'ArrowRight') {
        e.preventDefault();
        if (currentStep < totalSteps) {
            changeStep(1);
        }
    }
    
    // Alt + Left Arrow - Previous step
    if (e.altKey && e.key === 'ArrowLeft') {
        e.preventDefault();
        if (currentStep > 1) {
            changeStep(-1);
        }
    }
    
    // Ctrl/Cmd + S - Save draft
    if ((e.ctrlKey || e.metaKey) && e.key === 's') {
        e.preventDefault();
        saveDraft();
    }
});

// Auto-save draft every 2 minutes
setInterval(() => {
    const title = document.querySelector('[name="Title"]')?.value;
    if (title && title.trim()) {
        console.log('Auto-saving draft...');
        // Implement auto-save logic here
    }
}, 120000);
// Lesson Management JavaScript

document.addEventListener('DOMContentLoaded', function() {
    initializeLessonManagement();
});

function initializeLessonManagement() {
    // Initialize form handlers
    initializeAddLessonForm();
    initializeEditLessonForm();
    initializeMaterialUpload();
    
    // Initialize drag and drop for lesson reordering
    initializeDragAndDrop();
    
    // Initialize YouTube URL validation
    initializeYouTubeValidation();
}

// Add Lesson Form Handler
function initializeAddLessonForm() {
    const addLessonForm = document.getElementById('addLessonForm');
    if (addLessonForm) {
        addLessonForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const submitBtn = this.querySelector('button[type="submit"]');
            const originalText = submitBtn.innerHTML;
            
            try {
                // Show loading state
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Adding...';
                submitBtn.disabled = true;
                
                // Handle material upload first if file is selected
                const materialFile = document.getElementById('materialFile');
                let materialUrl = '';
                
                if (materialFile.files.length > 0) {
                    materialUrl = await uploadMaterial(materialFile.files[0]);
                }
                
                // Prepare form data
                const formData = new FormData(this);
                if (materialUrl) {
                    formData.set('MaterialUrl', materialUrl);
                }
                
                // Convert to JSON
                const lessonData = {
                    Title: formData.get('Title'),
                    Description: formData.get('Description'),
                    VideoUrl: formData.get('VideoUrl'),
                    MaterialUrl: materialUrl,
                    OrderIndex: parseInt(formData.get('OrderIndex')),
                    Duration: parseInt(formData.get('Duration')),
                    CourseId: parseInt(formData.get('CourseId'))
                };
                
                // Submit lesson
                const response = await fetch('/instructor/create-lesson', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                    },
                    body: JSON.stringify(lessonData)
                });
                
                const result = await response.json();
                
                if (result.success) {
                    // Close modal and reload page
                    const modal = bootstrap.Modal.getInstance(document.getElementById('addLessonModal'));
                    modal.hide();
                    
                    showNotification('Lesson added successfully!', 'success');
                    setTimeout(() => location.reload(), 1000);
                } else {
                    showNotification(result.message || 'Error adding lesson', 'error');
                }
                
            } catch (error) {
                console.error('Error adding lesson:', error);
                showNotification('An error occurred while adding the lesson', 'error');
            } finally {
                // Reset button
                submitBtn.innerHTML = originalText;
                submitBtn.disabled = false;
            }
        });
    }
}

// Edit Lesson Form Handler
function initializeEditLessonForm() {
    const editLessonForm = document.getElementById('editLessonForm');
    if (editLessonForm) {
        editLessonForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const submitBtn = this.querySelector('button[type="submit"]');
            const originalText = submitBtn.innerHTML;
            
            try {
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Updating...';
                submitBtn.disabled = true;
                
                // Handle material upload if new file is selected
                const materialFile = document.getElementById('editMaterialFile');
                let materialUrl = '';
                
                if (materialFile.files.length > 0) {
                    materialUrl = await uploadMaterial(materialFile.files[0]);
                }
                
                const formData = new FormData(this);
                const lessonData = {
                    Id: parseInt(formData.get('Id')),
                    Title: formData.get('Title'),
                    Description: formData.get('Description'),
                    VideoUrl: formData.get('VideoUrl'),
                    MaterialUrl: materialUrl || formData.get('CurrentMaterialUrl'),
                    OrderIndex: parseInt(formData.get('OrderIndex')),
                    Duration: parseInt(formData.get('Duration')),
                    CourseId: parseInt(formData.get('CourseId'))
                };
                
                const response = await fetch('/instructor/update-lesson', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                    },
                    body: JSON.stringify(lessonData)
                });
                
                const result = await response.json();
                
                if (result.success) {
                    const modal = bootstrap.Modal.getInstance(document.getElementById('editLessonModal'));
                    modal.hide();
                    
                    showNotification('Lesson updated successfully!', 'success');
                    setTimeout(() => location.reload(), 1000);
                } else {
                    showNotification(result.message || 'Error updating lesson', 'error');
                }
                
            } catch (error) {
                console.error('Error updating lesson:', error);
                showNotification('An error occurred while updating the lesson', 'error');
            } finally {
                submitBtn.innerHTML = originalText;
                submitBtn.disabled = false;
            }
        });
    }
}

// Material Upload Handler
function initializeMaterialUpload() {
    const materialInputs = document.querySelectorAll('input[type="file"][accept*=".pdf"]');
    
    materialInputs.forEach(input => {
        input.addEventListener('change', function() {
            const file = this.files[0];
            if (file) {
                validateMaterialFile(file);
            }
        });
    });
}

async function uploadMaterial(file) {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('courseId', document.querySelector('input[name="CourseId"]').value);
    
    const response = await fetch('/instructor/upload-material', {
        method: 'POST',
        body: formData
    });
    
    const result = await response.json();
    
    if (result.success) {
        return result.url;
    } else {
        throw new Error(result.message || 'Failed to upload material');
    }
}

function validateMaterialFile(file) {
    const allowedTypes = ['.pdf', '.doc', '.docx'];
    const fileExtension = '.' + file.name.split('.').pop().toLowerCase();
    
    if (!allowedTypes.includes(fileExtension)) {
        showNotification('Only PDF and DOC files are allowed', 'error');
        return false;
    }
    
    if (file.size > 10 * 1024 * 1024) { // 10MB limit
        showNotification('File size must be less than 10MB', 'error');
        return false;
    }
    
    return true;
}

// YouTube URL Validation
function initializeYouTubeValidation() {
    const youtubeInputs = document.querySelectorAll('input[name="VideoUrl"]');
    
    youtubeInputs.forEach(input => {
        input.addEventListener('blur', function() {
            const url = this.value.trim();
            if (url && !isValidYouTubeUrl(url)) {
                showNotification('Please enter a valid YouTube URL', 'warning');
                this.classList.add('is-invalid');
            } else {
                this.classList.remove('is-invalid');
                if (url) {
                    previewYouTubeVideo(url, this);
                }
            }
        });
    });
}

function isValidYouTubeUrl(url) {
    const youtubeRegex = /^(https?:\/\/)?(www\.)?(youtube\.com\/(watch\?v=|embed\/)|youtu\.be\/)[\w-]+/;
    return youtubeRegex.test(url);
}

function previewYouTubeVideo(url, input) {
    const videoId = extractYouTubeVideoId(url);
    if (videoId) {
        // Create preview thumbnail
        const preview = document.createElement('div');
        preview.className = 'youtube-preview mt-2';
        preview.innerHTML = `
            <img src="https://img.youtube.com/vi/${videoId}/mqdefault.jpg" 
                 alt="Video preview" 
                 style="width: 120px; height: 68px; border-radius: 4px;">
        `;
        
        // Remove existing preview
        const existingPreview = input.parentNode.querySelector('.youtube-preview');
        if (existingPreview) {
            existingPreview.remove();
        }
        
        // Add new preview
        input.parentNode.appendChild(preview);
    }
}

function extractYouTubeVideoId(url) {
    const regex = /(?:youtube\.com\/(?:[^\/]+\/.+\/|(?:v|e(?:mbed)?)\/|.*[?&]v=)|youtu\.be\/)([^"&?\/\s]{11})/;
    const match = url.match(regex);
    return match ? match[1] : null;
}

// Drag and Drop for Lesson Reordering
function initializeDragAndDrop() {
    const lessonsList = document.getElementById('lessonsList');
    if (lessonsList) {
        new Sortable(lessonsList, {
            handle: '.lesson-drag-handle',
            animation: 150,
            ghostClass: 'lesson-ghost',
            chosenClass: 'lesson-chosen',
            dragClass: 'lesson-drag',
            onEnd: function(evt) {
                updateLessonOrder();
            }
        });
    }
}

async function updateLessonOrder() {
    const lessonItems = document.querySelectorAll('.lesson-item');
    const orderData = [];
    
    lessonItems.forEach((item, index) => {
        const lessonId = item.getAttribute('data-lesson-id');
        orderData.push({
            id: parseInt(lessonId),
            orderIndex: index + 1
        });
    });
    
    try {
        const response = await fetch('/instructor/update-lesson-order', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify(orderData)
        });
        
        const result = await response.json();
        
        if (result.success) {
            showNotification('Lesson order updated', 'success');
            // Update lesson numbers in UI
            updateLessonNumbers();
        } else {
            showNotification('Failed to update lesson order', 'error');
        }
    } catch (error) {
        console.error('Error updating lesson order:', error);
        showNotification('An error occurred while updating lesson order', 'error');
    }
}

function updateLessonNumbers() {
    const lessonItems = document.querySelectorAll('.lesson-item');
    lessonItems.forEach((item, index) => {
        const numberSpan = item.querySelector('.lesson-number');
        if (numberSpan) {
            numberSpan.textContent = (index + 1) + '.';
        }
    });
}

// Edit Lesson Function
function editLesson(lessonId) {
    // Fetch lesson data and populate edit modal
    fetch(`/instructor/get-lesson/${lessonId}`)
        .then(response => response.json())
        .then(lesson => {
            if (lesson) {
                document.getElementById('editLessonId').value = lesson.id;
                document.getElementById('editLessonTitle').value = lesson.title;
                document.getElementById('editLessonDescription').value = lesson.description;
                document.getElementById('editLessonVideoUrl').value = lesson.videoUrl || '';
                document.getElementById('editLessonOrder').value = lesson.orderIndex;
                document.getElementById('editLessonDuration').value = lesson.duration;
                
                // Show modal
                const modal = new bootstrap.Modal(document.getElementById('editLessonModal'));
                modal.show();
            }
        })
        .catch(error => {
            console.error('Error fetching lesson:', error);
            showNotification('Error loading lesson data', 'error');
        });
}

// Delete Lesson Function
function deleteLesson(lessonId) {
    if (confirm('Are you sure you want to delete this lesson? This action cannot be undone.')) {
        fetch(`/instructor/delete-lesson/${lessonId}`, {
            method: 'DELETE',
            headers: {
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            }
        })
        .then(response => response.json())
        .then(result => {
            if (result.success) {
                showNotification('Lesson deleted successfully', 'success');
                setTimeout(() => location.reload(), 1000);
            } else {
                showNotification(result.message || 'Error deleting lesson', 'error');
            }
        })
        .catch(error => {
            console.error('Error deleting lesson:', error);
            showNotification('An error occurred while deleting the lesson', 'error');
        });
    }
}

// Reorder Lessons Function
function reorderLessons() {
    const lessonsList = document.getElementById('lessonsList');
    if (lessonsList.classList.contains('reorder-mode')) {
        // Exit reorder mode
        lessonsList.classList.remove('reorder-mode');
        showNotification('Lesson reordering disabled', 'info');
    } else {
        // Enter reorder mode
        lessonsList.classList.add('reorder-mode');
        showNotification('Drag lessons to reorder them', 'info');
    }
}

// Notification System
function showNotification(message, type = 'info') {
    // Remove existing notifications
    const existingNotifications = document.querySelectorAll('.notification');
    existingNotifications.forEach(notification => notification.remove());
    
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `notification alert alert-${type === 'error' ? 'danger' : type} alert-dismissible fade show`;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        min-width: 300px;
        animation: slideInRight 0.3s ease-out;
    `;
    
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    document.body.appendChild(notification);
    
    // Auto-remove after 5 seconds
    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 5000);
}

// Utility Functions
function formatDuration(minutes) {
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return hours > 0 ? `${hours}:${mins.toString().padStart(2, '0')}` : `${mins}:00`;
}

function validateForm(form) {
    const requiredFields = form.querySelectorAll('[required]');
    let isValid = true;
    
    requiredFields.forEach(field => {
        if (!field.value.trim()) {
            field.classList.add('is-invalid');
            isValid = false;
        } else {
            field.classList.remove('is-invalid');
        }
    });
    
    return isValid;
}

// CSS for drag and drop states
const style = document.createElement('style');
style.textContent = `
    .lesson-ghost {
        opacity: 0.5;
        background: #e3f2fd;
    }
    
    .lesson-chosen {
        box-shadow: 0 8px 25px rgba(0,0,0,0.15);
    }
    
    .lesson-drag {
        transform: rotate(5deg);
    }
    
    .reorder-mode .lesson-drag-handle {
        color: #007bff;
        cursor: grabbing;
    }
    
    .reorder-mode .lesson-item {
        border: 2px dashed #007bff;
    }
    
    @keyframes slideInRight {
        from {
            opacity: 0;
            transform: translateX(100%);
        }
        to {
            opacity: 1;
            transform: translateX(0);
        }
    }
`;
document.head.appendChild(style);

// Load Sortable.js if not already loaded
if (typeof Sortable === 'undefined') {
    const script = document.createElement('script');
    script.src = 'https://cdn.jsdelivr.net/npm/sortablejs@1.15.0/Sortable.min.js';
    script.onload = () => {
        console.log('Sortable.js loaded');
        initializeDragAndDrop();
    };
    document.head.appendChild(script);
}
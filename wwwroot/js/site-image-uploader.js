document.addEventListener("DOMContentLoaded", function () {
    FilePond.registerPlugin(FilePondPluginImagePreview, FilePondPluginFileValidateType, FilePondPluginFileMetadata);

    document.querySelectorAll('.filepond').forEach(input => {
        const categoryId = input.dataset.categoryId;
        const siteId = document.getElementById("SiteImageUploaderVM_SiteId").value;

        FilePond.create(input, {
            allowMultiple: true,
            allowProcess: false, // Prevent automatic upload
            labelIdle: 'Drag & Drop files or <span class="filepond--label-action">Browse</span>',

            // Load initial images from the server
            files: sitePicCategoryList
                .find(c => c.picCatId == categoryId)?.images.map(image => ({
                    source: image.filePath,
                    options: {
                        type: 'local',
                        file: {
                            name: image.fileName,
                            size: image.size || 200000, // Use actual size if available, else default
                            type: 'image/png' // Ensure this matches the actual file type
                        },
                        metadata: {
                            comment: image.fileDescription || '',
                            uniqueFileId: image.sitePicID
                        }
                    }
                })) || [],

            // Upload new images to the server
            server: {
                process: (fieldName, file, metadata, load, error, progress, abort) => {
                    const formData = new FormData();
                    formData.append('imageFile', file);
                    formData.append('categoryId', categoryId);
                    formData.append('siteId', siteId);
                    formData.append('description', metadata.comment || '');

                    const request = new XMLHttpRequest();
                    request.open('POST', '/api/Navigation/UploadSiteImage');
                    request.setRequestHeader('Authorization', `Bearer ${tokenC}`);

                    request.upload.onprogress = (e) => progress(e.lengthComputable, e.loaded, e.total);

                    request.onload = function () {
                        if (request.status === 200) {
                            const response = JSON.parse(request.responseText);
                            load(response.filePath);

                            // Trigger success message and update UI
                            toastr.success(`File uploaded successfully: ${response.filePath}`);

                            // Store the sitePicId (uniqueFileId) in the file's metadata
                            file.setMetadata('uniqueFileId', response.sitePicId);

                            input.dispatchEvent(new CustomEvent('file-uploaded', {
                                detail: { uniqueFileId: response.sitePicId, filePath: response.filePath }
                            }));
                        } else {
                            error('Upload failed');
                            toastr.error('Upload failed. Please try again.');
                        }
                    };

                    request.onerror = () => {
                        error('Network error');
                        toastr.error('Network error. Please check your connection.');
                    };

                    request.onabort = () => abort();
                    request.send(formData);
                },

                // Revert file removal by sending delete API
                revert: (uniqueFileId, load) => {
                    if (!uniqueFileId) return load();

                    $.ajax({
                        url: `/api/Navigation/DeleteSiteImage?imageId=${uniqueFileId}`,
                        type: 'DELETE',
                        headers: {
                            'Authorization': `Bearer ${tokenC}`
                        },
                        success: function () {
                            // Trigger success toastr notification after deletion
                            toastr.success('Image deleted successfully');
                            load(); // Proceed with removing the file from UI
                        },
                        error: function (err) {
                            toastr.error('Failed to delete image. Please try again.');
                        }
                    });
                }
            },

            onprocessfile: (error, file) => {
                const status = file.getMetadata('status');
                if (status === 'success') {
                    file.status = FilePond.FileStatus.PROCESSING_COMPLETE;
                } else if (status === 'error') {
                    file.status = FilePond.FileStatus.PROCESSING_ERROR;
                }
            },

            onremovefile: (error, file) => {
                const uniqueFileId = file.getMetadata('uniqueFileId');
                if (uniqueFileId) {
                    $.ajax({
                        url: `/api/Navigation/DeleteSiteImage?imageId=${uniqueFileId}`,
                        type: 'DELETE',
                        headers: {
                            'Authorization': `Bearer ${tokenC}`
                        },
                        success: () => {
                            toastr.success('Image deleted successfully');
                        },
                        error: err => {
                            toastr.error('Failed to delete preloaded image');
                        }
                    });
                }
            },

            onaddfile: (error, fileItem) => {
                const commentInput = document.createElement('input');
                commentInput.type = 'text';
                commentInput.placeholder = 'Add a description';
                commentInput.classList.add('comment-input');

                // Set initial value if available
                const comment = fileItem.getMetadata('comment');
                if (comment) commentInput.value = comment;

                // Update metadata with user input
                commentInput.addEventListener('input', () => {
                    fileItem.setMetadata('comment', commentInput.value);
                });

                // Append the input to the FilePond file item
                /*const fileItemElement = fileItem.element.querySelector('.filepond--file-info');
                if (fileItemElement) fileItemElement.appendChild(commentInput);*/
            }
        });
    });
});

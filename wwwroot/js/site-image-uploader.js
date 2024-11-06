document.addEventListener("DOMContentLoaded", function () {
    FilePond.registerPlugin(FilePondPluginImagePreview);
    document.querySelectorAll('.filepond').forEach(input => {
        const categoryId = input.dataset.categoryId;

        // Initialize FilePond
        FilePond.create(input, {
            allowMultiple: true,
            server: {
                process: (file, metadata, load, error, progress, abort) => {
                    // Prepare form data with required parameters
                    const formData = new FormData();
                    formData.append('imageFile', file);
                    formData.append('categoryId', categoryId);
                    formData.append('siteId', $('#SiteImageUploaderVM_SiteId').val());

                    const request = new XMLHttpRequest();
                    request.open('POST', `/api/Navigation/UploadSiteImage`);
                    request.setRequestHeader('Authorization', 'Bearer ' + tokenC);

                    // Update progress and handle load/error
                    request.upload.onprogress = (e) => {
                        progress(e.lengthComputable, e.loaded, e.total);
                    };
                    request.onload = function () {
                        if (request.status === 200) {
                            load(request.responseText);  // Notify FilePond of completion
                            console.log('Image uploaded successfully:', request.responseText);
                        } else {
                            error('Upload failed');  // Notify FilePond of error
                        }
                    };
                    request.onerror = () => error('Upload failed');
                    request.onabort = () => abort(); // Handle abort signal

                    // Send form data
                    request.send(formData);
                },
                revert: (uniqueFileId, load) => {
                    // Delete image with specified ID
                    $.ajax({
                        url: `/api/Navigation/DeleteSiteImage?imageId=${uniqueFileId}`,
                        type: 'DELETE',
                        headers: {
                            'Authorization': 'Bearer ' + token
                        },
                        success: () => {
                            load();
                            console.log('Image deleted successfully');
                        },
                        error: err => console.error('Delete error:', err)
                    });
                }
            }
        });
    });
    // Handle image deletion with SitePicID
    document.querySelectorAll('.delete-image').forEach(button => {
        button.addEventListener('click', function () {
            const imageId = this.dataset.imageId;

            $.ajax({
                url: `/api/Navigation/DeleteSiteImage?imageId=${imageId}`,
                type: 'DELETE',
                headers: {
                    'Authorization': 'Bearer ' + tokenC
                },
                success: () => {
                    console.log('Image deleted');
                    this.closest('li').remove(); // Remove image from the list
                },
                error: err => console.error('Delete error:', err)
            });
        });
    });
});

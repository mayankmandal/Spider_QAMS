async function fetchProfiles() {
    $.ajax({
        url: '/api/Navigation/GetCurrentUserProfile',
        type: 'GET',
        headers: {
            'Authorization': 'Bearer ' + tokenC
        },
        success: function (profile) {
            populateSidebarforProfiles(profile);
        },
        error: function (xhr, status, error) {
            console.error('Fetch error: ', error);
        }
    });
}
function populateSidebarforProfiles(profile) {
    const list = document.getElementById('dynamicProfileName');
    list.innerHTML = ''; // Clear existing list items if any

    const listItem = document.createElement('li');
    listItem.className = 'nav-item';

    const link = document.createElement('a');
    link.href = '/ReadUserProfile';
    link.className = 'nav-link';
    link.target = '_self';

    const icon = document.createElement('i');
    icon.className = 'nav-icon fas fa-user';

    const paragraph = document.createElement('p');

    const text = document.createTextNode(profile.profileName);

    paragraph.appendChild(text);
    link.appendChild(icon);
    link.appendChild(paragraph);
    listItem.appendChild(link);
    list.appendChild(listItem);
}

async function fetchPages() {
    $.ajax({
        url: '/api/Navigation/GetCurrentUserPages',
        type: 'GET',
        headers: {
            'Authorization': 'Bearer ' + tokenC
        },
        success: function (pages) {
            pages.sort((a, b) => a.pageDesc.localeCompare(b.pageDescription));
            populateSidebarforPages(pages);
        },
        error: function (xhr, status, error) {
            console.error('Fetch error: ', error);
        }
    });
}
function populateSidebarforPages(pages) {
    const list = document.getElementById('dynamicPagesList');
    list.innerHTML = ''; // Clear existing list items if any
    pages.forEach(page => {
        const listItem = document.createElement('li');
        listItem.className = 'nav-item';

        const link = document.createElement('a');
        link.href = page.pageUrl;
        link.className = 'nav-link';
        link.target = '_self';

        const icon = document.createElement('i');
        icon.className = 'nav-icon fas fa-external-link-alt';

        const paragraph = document.createElement('p');

        const text = document.createTextNode(page.pageDesc);

        paragraph.appendChild(text);
        link.appendChild(icon);
        link.appendChild(paragraph);
        listItem.appendChild(link);
        list.appendChild(listItem);
    });
}

async function fetchCategories() {
    $.ajax({
        url: '/api/Navigation/GetCurrentUserCategories',
        type: 'GET',
        headers: {
            'Authorization': 'Bearer ' + tokenC
        },
        success: function (structureData) {
            // Sort the pages within each category (if not already sorted by the API)
            structureData.forEach(category => {
                category.pages.sort((a, b) => a.pageDesc.localeCompare(b.pageDesc));
            });

            // Sort the categories (if not already sorted by the API)
            structureData.sort((a, b) => a.categoryName.localeCompare(b.categoryName));

            populateSidebarforCategories(structureData);
        },
        error: function (xhr, status, error) {
            console.error('Fetch error: ', error);
        }
    });
}
function populateSidebarforCategories(categories) {
    const list = document.getElementById('dynamicCategoriesList');
    list.innerHTML = ''; // Clear existing list items if any
    categories.forEach(category => {
        const listItem = document.createElement('li');
        listItem.className = 'nav-item';

        const link = document.createElement('a');
        link.href = '#'; // Link to a placeholder since it's a submenu
        link.className = 'nav-link';
        link.target = '_self';
        link.innerHTML = `
                <i class="fas fa-atom nav-icon"></i>
                <p>${category.categoryName}<i class="fas fa-angle-left right"></i></p>
            `;

        const submenu = document.createElement('ul');
        submenu.className = 'nav nav-treeview ms-2';
        submenu.style.borderLeft = '1px ridge #DDDFBF';

        // Populate pages under this category
        category.pages.forEach(page => {
            const pageItem = document.createElement('li');
            pageItem.className = 'nav-item';

            const pageLink = document.createElement('a');
            pageLink.target = '_self';
            pageLink.href = page.pageUrl;
            pageLink.className = 'nav-link';
            pageLink.innerHTML = `
                    <i class="nav-icon fas fa-link"></i>
                    <p>${page.pageDesc}</p>
                `;

            pageItem.appendChild(pageLink);
            submenu.appendChild(pageItem);
        });

        listItem.appendChild(link);
        listItem.appendChild(submenu);
        list.appendChild(listItem);
    });
}

async function fetchUserProfile() {

    $.ajax({
        url: '/api/Navigation/GetCurrentUser',
        headers: {
            'Authorization': 'Bearer ' + tokenC
        },
        type: 'GET',
        dataType: 'json',
        success: function (profile) {
            populateSidebarforUserProfiles(profile);
        },
        error: function (xhr, status, error) {
            console.error('Fetch error:', status, error);
        }
    });
}
function populateSidebarforUserProfiles(currentUser) {
    // Update the username in the sidebar
    $('#userNameDiv a').text(currentUser.userName);

    // Update the user image in the sidebar
    $('#userImageDiv img').attr('src', currentUser.profilePicName);
    $('#userImageDiv img').attr('alt', `Image of ${currentUser.userName}`);

    $('#userImageDiv p').text(currentUser.fullName + ' - ' + currentUser.designation);
    $('#userImageDiv span').text(currentUser.fullName);
}

document.addEventListener('DOMContentLoaded', function () {
    fetchUserProfile();
    fetchProfiles();
    fetchPages();
    fetchCategories();
});
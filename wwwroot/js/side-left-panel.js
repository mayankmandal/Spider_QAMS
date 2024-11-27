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
    // Find the "My Profile" link
    const myProfileLink = document.querySelector('#collapseExample .nav-link');

    if (myProfileLink) {
        // Set the hyperlink for the "My Profile" link
        myProfileLink.href = '/ViewUserProfile';

        // Update the mini value (MP) based on the profile
        const miniSpan = myProfileLink.querySelector('.sidebar-mini');
        if (miniSpan) {
            miniSpan.textContent = profile.profileName ? getAbbreviation('Profile ' + profile.profileName) : 'PR';
        }

        // Update the full profile name in the sidebar-normal span
        const normalSpan = myProfileLink.querySelector('.sidebar-normal');
        if (normalSpan) {
            normalSpan.textContent = `Profile - ${profile.profileName}`;
        }
    }

    
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

    // Add the static Dashboard item
    const dashboardItem = document.createElement('li');
    dashboardItem.className = 'nav-item active';

    const dashboardLink = document.createElement('a');
    dashboardLink.className = 'nav-link';
    dashboardLink.href = '/Dashboard';
    dashboardLink.innerHTML = `
        <i class="material-icons">dashboard</i>
        <p> Dashboard </p>
    `;
    dashboardItem.appendChild(dashboardLink);
    list.appendChild(dashboardItem);

    // Loop over categories to dynamically create other items
    categories.forEach((category, index) => {
        // Main category item with collapse functionality
        const listItem = document.createElement('li');
        listItem.className = 'nav-item';

        const link = document.createElement('a');
        link.href = `#categoryCollapse${index}`; // Unique ID for each category collapse
        link.className = 'nav-link';
        link.setAttribute('data-toggle', 'collapse');

        link.innerHTML = `
            <i class="material-icons">apps</i>
            <p>
                ${category.categoryName}
                <b class="caret"></b>
            </p>
        `;

        // Create the collapse container for category pages
        const collapseDiv = document.createElement('div');
        collapseDiv.className = 'collapse';
        collapseDiv.id = `categoryCollapse${index}`; // ID matches the href above

        const submenu = document.createElement('ul');
        submenu.className = 'nav';

        // Populate pages under this category
        category.pages.forEach(page => {
            const pageItem = document.createElement('li');
            pageItem.className = 'nav-item';

            const pageLink = document.createElement('a');
            pageLink.target = '_self';
            pageLink.href = page.pageUrl;
            pageLink.className = 'nav-link';

            pageLink.innerHTML = `
                <span class="sidebar-mini">${getAbbreviation(page.pageDesc)}</span>
                <span class="sidebar-normal">${page.pageDesc}</span>
            `;

            pageItem.appendChild(pageLink);
            submenu.appendChild(pageItem);
        });

        collapseDiv.appendChild(submenu);
        listItem.appendChild(link);
        listItem.appendChild(collapseDiv);
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

    $('#currentUserName span span').text(currentUser.fullName);
}

document.addEventListener('DOMContentLoaded', function () {
    fetchUserProfile();
    fetchProfiles();
    fetchCategories();
});
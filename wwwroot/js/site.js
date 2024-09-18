function getCurrentFormattedDateTime() {
    var currentDate = new Date();
    var monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    var month = monthNames[currentDate.getMonth()];
    var day = String(currentDate.getDate()).padStart(2, '0');
    var year = currentDate.getFullYear();
    var hours = String(currentDate.getHours()).padStart(2, '0');
    var minutes = String(currentDate.getMinutes()).padStart(2, '0');
    var seconds = String(currentDate.getSeconds()).padStart(2, '0');

    var formattedDateTime = `${month} ${day} ${year}T${hours}.${minutes}.${seconds}`;

    return formattedDateTime;
}

$(document).ready(function () {
    var currentYear = new Date().getFullYear();
    $('#copyright-year').text(currentYear);
    toastr.options = {
        "positionClass": "toast-bottom-right", // Set the position to right bottom
    };
});


$(document).on('click', '[data-toggle="password"]', function () {
    var icon = $(this);
    var input = icon.closest('.input-group').find('.password-toggle-input');

    if (input.attr('type') === 'password') {
        input.attr('type', 'text');
        icon.removeClass('fa-eye-slash').addClass('fa-eye');
    } else {
        input.attr('type', 'password');
        icon.removeClass('fa-eye').addClass('fa-eye-slash');
    }
});

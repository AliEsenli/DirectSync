window.addEventListener("load", function () {

    var password = document.getElementById("password");
    var labelPassword = document.getElementById("label-password");

    var email = document.getElementById("email");
    var labelEmail = document.getElementById("label-email");

    var confirm = document.getElementById("confirm-password");
    var labelConfirm = document.getElementById("label-confirm");

    if (email.value.length != 0) {
        labelEmail.classList.add("active");
    }

    password.addEventListener("focusin", function () {
        labelPassword.classList.add("active");
    });

    password.addEventListener("focusout", function () {
        if (password.value.length == 0)
            labelPassword.classList.remove("active");
    });

    email.addEventListener("focusin", function () {
        labelEmail.classList.add("active");
    });

    email.addEventListener("focusout", function () {
        if (email.value.length == 0)
            labelEmail.classList.remove("active");
    });

    if (confirm) {
        confirm.addEventListener("focusin", function () {
            labelConfirm.classList.add("active");
        });

        confirm.addEventListener("focusout", function () {
            labelConfirm.classList.remove("active");
        });
    }

});
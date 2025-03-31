// GSAP Animation for Register Card
document.addEventListener('DOMContentLoaded', function () {
    if (typeof gsap !== "undefined") {
        gsap.from(".register-card", {
            duration: 1,
            opacity: 0,
            y: 50,
            ease: "power2.out"
        });
    }
});

$(document).ready(function () {
    // Toggle password visibility
    $(".toggle-password").click(function () {
        var input = $("#" + $(this).data("target"));
        var icon = $(this).find("i");

        if (input.attr("type") === "password") {
            input.attr("type", "text");
            icon.removeClass("bi-eye").addClass("bi-eye-slash");
        } else {
            input.attr("type", "password");
            icon.removeClass("bi-eye-slash").addClass("bi-eye");
        }
    });

    // AJAX Form Submission for Registration
    $("#registerForm").submit(function (event) {
        event.preventDefault();
        $("#passwordMismatchMessage").hide();

        // Check if passwords match
        if ($("#password").val() !== $("#confirmPassword").val()) {
            $("#passwordMismatchMessage")
                .text("Password and Confirm Password do not match!")
                .show();
            return;
        }

        // Set loading state for the button
        var $registerBtn = $("#registerBtn");
        var originalText = $registerBtn.html();
        $registerBtn.prop("disabled", true).html(
            '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Registering...'
        );

        // Retrieve the register URL from the form's data attribute
        var registerUrl = $("#registerForm").data("register-url");

        $.ajax({
            url: registerUrl,
            type: "POST",
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message, "Success");
                    setTimeout(function () {
                        window.location.href = '/Account/Login';
                    }, 2000);
                } else {
                    toastr.error(response.message, "Error");
                }
                $registerBtn.prop("disabled", false).html(originalText);
            },
            error: function () {
                toastr.error("Something went wrong. Please try again!", "Error");
                $registerBtn.prop("disabled", false).html(originalText);
            }
        });
    });
});

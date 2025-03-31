document.addEventListener("DOMContentLoaded", function () {
    // Ensure GSAP and ScrollTrigger are available
    if (typeof gsap !== "undefined" && gsap.registerPlugin) {
        gsap.registerPlugin(ScrollTrigger);
    }

    // Animate Navbar
    //gsap.from("nav.navbar", { duration: 1, y: -100, ease: "bounce.out" });
    gsap.from(".register-card", { opacity: 0, y: -50, duration: 0.8 });

    // Animate Hero Sections
    gsap.from(".hero-content h1, .about-hero .hero-content h1, .contact-hero .hero-content h1", {
        duration: 1.2,
        opacity: 0,
        y: -50,
        ease: "power2.out"
    });
    gsap.from(".hero-content p, .about-hero .hero-content p, .contact-hero .hero-content p", {
        duration: 1,
        opacity: 0,
        y: 50,
        delay: 0.5,
        ease: "power2.out"
    });

    // Animate Search Bar
/*    gsap.from(".search-bar", { duration: 1, opacity: 0, scale: 0.8, delay: 1.2 });*/

    // Animate Section Titles on Scroll
    gsap.utils.toArray(".section-title").forEach((title) => {
        gsap.from(title, {
            scrollTrigger: {
                trigger: title,
                start: "top 85%"
            },
            duration: 0.8,
            opacity: 0,
            y: 30,
            ease: "power2.out"
        });
    });

    // Animate Team Cards on Scroll
    gsap.utils.toArray(".team-card").forEach((card) => {
        gsap.from(card, {
            scrollTrigger: {
                trigger: card,
                start: "top 90%"
            },
            duration: 0.8,
            opacity: 0,
            y: 30,
            ease: "power2.out"
        });
    });

    // Animate Footer on Scroll
    //gsap.from("footer", {
    //    scrollTrigger: {
    //        trigger: "footer",
    //        start: "top 80%"
    //    },
    //    duration: 1,
    //    opacity: 0,
    //    y: 50,
    //    ease: "power2.out"
    //});

    // Animate Contact Page Elements
    gsap.from(".contact-info", {
        scrollTrigger: {
            trigger: ".contact-info",
            start: "top 85%"
        },
        duration: 1,
        opacity: 0,
        x: -50,
        ease: "power2.out"
    });

    gsap.from(".contact-form", {
        scrollTrigger: {
            trigger: ".contact-form",
            start: "top 85%"
        },
        duration: 1,
        opacity: 0,
        x: 50,
        ease: "power2.out"
    });
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

    // AJAX Login Submission
    $("#loginForm").submit(function (event) {
        event.preventDefault();
        var $form = $(this);
        var $loginBtn = $("#loginBtn");
        var originalBtnContent = $loginBtn.html();
        var loginUrl = $("#loginForm").data("login-url") || "/Account/Login"; // Ensure correct login URL
        var returnUrl = $("#returnUrl").val() || "Home/Index"; // Get return URL or set default

        $loginBtn.prop("disabled", true).html(
            '<span class="spinner-border spinner-border-sm"></span> Logging in...'
        );

        $.ajax({
            url: loginUrl,
            type: "POST",
            data: $form.serialize() + "&returnUrl=" + encodeURIComponent(returnUrl), // Include returnUrl
            success: function (response) {
                console.log("Redirect URL:", response.redirectUrl);
                if (response.success) {
                    toastr.success(response.message, "Success");
                    setTimeout(() => window.location.href = response.redirectUrl, 1500);
                } else {
                    toastr.error(response.message, "Error");
                }
            },
            error: function () {
                toastr.error("Something went wrong. Please try again!", "Error");
            },
            complete: function () {
                $loginBtn.prop("disabled", false).html(originalBtnContent);
            }
        });
    });
});

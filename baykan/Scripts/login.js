$(document).ready(function () {
    $("#loginForm").submit(function (event) {
        event.preventDefault(); // Stop full page reload

        $.ajax({
            type: "POST",
            url: "/Account/Login",
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    window.location.href = response.redirectUrl; // Redirect on success
                } else {
                    $("#loginError").text(response.message).show(); // Make sure the error appears inside modal
                }
            },
            error: function () {
                $("#loginError").text("Something went wrong. Please try again.").show();
            }
        });
    });
});

$(document).ready(function () {
    // Handle Buy Now & Add to Cart clicks when not logged in
    $(".login-trigger").click(function (event) {
        event.preventDefault(); // Prevent default action

        var isAuthenticated = '@(Session["UserId"] != null ? "true" : "false")'; // Check if user is logged in

        if (isAuthenticated === "false") {
            var url = $(this).data("url"); // Store the clicked URL
            $("#loginModal").modal("show"); // Show login modal

            // Store the URL in session storage to redirect after login
            sessionStorage.setItem("postLoginRedirect", url);
        } else {
            window.location.href = $(this).data("url"); // Proceed to cart
        }
    });

    // After login, redirect to stored URL
    if (sessionStorage.getItem("postLoginRedirect")) {
        var redirectUrl = sessionStorage.getItem("postLoginRedirect");
        sessionStorage.removeItem("postLoginRedirect"); // Clear storage
        window.location.href = redirectUrl; // Redirect to saved URL
    }
});


<script>
    function addToCart(productId) {
        window.location.href = "/Cart/AddToCart?productId=" + productId;
    }

    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll(".login-trigger").forEach(item => {
            item.addEventListener("click", function (event) {
                event.preventDefault();
                $("#loginModal").modal("show");
            });
        });
    });
</script>

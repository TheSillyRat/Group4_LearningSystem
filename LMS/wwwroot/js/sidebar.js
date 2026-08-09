document.addEventListener("DOMContentLoaded", function () {

    const toggleButtons = document.querySelectorAll("[data-sidebar-toggle]");

    toggleButtons.forEach(function (button) {

        button.addEventListener("click", function () {

            const submenu = button.nextElementSibling;
            const arrow = button.querySelector(".sidebar-arrow");

            if (!submenu) {
                return;
            }

            submenu.classList.toggle("open");

            if (arrow) {
                arrow.style.transform =
                    submenu.classList.contains("open")
                        ? "rotate(180deg)"
                        : "rotate(0deg)";
            }
        });

    });

});
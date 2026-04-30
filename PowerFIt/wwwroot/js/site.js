document.addEventListener("DOMContentLoaded", function () {
    const toggleBtn = document.getElementById("theme-toggle");
    const root = document.documentElement;

    if (!toggleBtn) return;

    if (localStorage.getItem("theme") === "light") {
        root.classList.add("light");
        toggleBtn.textContent = "🌙";
    } else {
        root.classList.remove("light");
        toggleBtn.textContent = "☀️";
    }

    toggleBtn.addEventListener("click", function () {
        root.classList.toggle("light");

        if (root.classList.contains("light")) {
            localStorage.setItem("theme", "light");
            toggleBtn.textContent = "🌙";
        } else {
            localStorage.setItem("theme", "dark");
            toggleBtn.textContent = "☀️";
        }
    });
});
window.addEventListener("load", function () {
    var toggler = document.getElementById("sidebar-toggler").addEventListener("click", function () {
        document.body.classList.toggle("drawer-sidebar");
    });

    var x = document.getElementById("body");
    if (window.innerWidth < 650) {
        x.classList.add("drawer-sidebar");
    }

    window.addEventListener("resize", function () {
        if (window.innerWidth < 650) {
            x.classList.add("drawer-sidebar");
        }
    });
});

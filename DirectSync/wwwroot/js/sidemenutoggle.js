window.addEventListener("load", function () {
    var content = document.getElementById("content-area");

    document.getElementById("sidebar-toggler").addEventListener("click", function () {
        var windowSize = window.innerWidth;
        if (windowSize > 650 && !document.body.classList.contains("drawer-sidebar")) {
            content.style = "padding-left: 0px;";
            document.body.classList.toggle("drawer-sidebar");
        } else if (windowSize > 650 && document.body.classList.contains("drawer-sidebar")) {
            document.body.classList.toggle("drawer-sidebar");
            content.style = "padding-left: 255px;";
        } else {
            document.body.classList.toggle("drawer-sidebar");
            content.style = "padding-left: 0px;";
        }
    });

    var x = document.getElementById("body");
    if (window.innerWidth < 650) {
        x.classList.add("drawer-sidebar");
    }

    window.addEventListener("resize", function () {
        if (window.innerWidth < 650) {
            content.style = "padding-left: 0px;";
            x.classList.add("drawer-sidebar");
        } else if (window.innerWidth > 650 && document.body.classList.contains("drawer-sidebar")) {
            content.style = "padding-left: 0px;";
        } else {
            content.style = "padding-left: 255px;";
        }
    });
});

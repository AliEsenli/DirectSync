window.addEventListener("load", function () {
    $(window).scroll(parallax);

    function parallax() {
        var scrollTop = $(window).scrollTop();
        var offsetY = scrollTop / 3;
        var position = 'center ' + '-' + offsetY + 'px';

        $(".landing").css("background-position", position);
        $(".landing").css("-webkit-background-position", position);
    }
});
$('.image-toggler').click(function () {
    if ($(".image").is(":visible")) {
        $(".image").hide();
        $(".image-tag").show();
    } else {
        $(".image").show();
        $(".image-tag").hide();
    }
});

$('#exampleModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget) // Button that triggered the modal
    var photoId = button.data('photo-id') // Extract info from data-* attributes

    var modal = $(this);
    modal.data("photo-id", photoId);
})

$('#modal-button-remove').bind('click', function (e) {
    var photoId = $("#exampleModal").data("photo-id");

    $.ajax({
        url: '/api/photo/' + photoId,
        dataType: "json",
        type: 'DELETE',
        success: function (result) {
            location.reload();
        }
    });
});
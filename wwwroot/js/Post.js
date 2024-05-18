document.addEventListener('DOMContentLoaded', function () {
    var likeButton = document.getElementById('likeButton');
    likeButton.addEventListener('click', function (event) {
        event.preventDefault();
        document.getElementById('likeForm').submit(); 
    });
});


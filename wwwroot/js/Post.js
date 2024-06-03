document.addEventListener('DOMContentLoaded', function () {
    var likeButton = document.getElementById('likeButton');
    likeButton.addEventListener('click', function (event) {
        event.preventDefault();
        document.getElementById('likeForm').submit();
    });
});

function copyPostUrl() {
    const postUrl = window.location.href;
    navigator.clipboard.writeText(postUrl).then(() => {
        const shareButton = document.getElementById('shareButton');
        shareButton.innerHTML = '<i class="fa-solid fa-check"></i> Link Copied!';
        setTimeout(() => {
            shareButton.innerHTML = '<i class="fa-solid fa-share"></i> Share Post';
        }, 2000);
    }).catch(err => {
        console.error('Failed to copy: ', err);
    });
}

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

 function editComment(commentId) {
            document.getElementById(`comment-text-${commentId}`).classList.add('d-none');
            document.getElementById(`edit-comment-${commentId}`).classList.remove('d-none');
        }

        function cancelEdit(commentId) {
            document.getElementById(`comment-text-${commentId}`).classList.remove('d-none');
            document.getElementById(`edit-comment-${commentId}`).classList.add('d-none');
        }
   
       
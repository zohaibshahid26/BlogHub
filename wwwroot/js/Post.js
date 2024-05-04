function toggleLike(icon) {
  if (icon.classList.contains("fa-regular")) {
    icon.classList.remove("fa-regular");
    icon.classList.add("fa-solid");
    updateLikeCount(1);
  } else {
    icon.classList.remove("fa-solid");
    icon.classList.add("fa-regular");
    updateLikeCount(-1);
  }
}

function updateLikeCount(count) {
  const likeCountElement = document.getElementById("like-count");
  const currentCount = parseInt(likeCountElement.innerText);
  likeCountElement.innerText = currentCount + count;
}

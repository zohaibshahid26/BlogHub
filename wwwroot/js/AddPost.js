document.getElementById("addTagButton").addEventListener("click", function () {
    const tagInput = document.getElementById("tagInput");
    const tag = tagInput.value.trim();
    const tagContainer = document.getElementById("tagContainer");
    const hiddenTags = document.getElementById("hiddenTags");

    const tagElements = tagContainer.getElementsByClassName("badge btn-primary fs-6 m-1");

    if (tag && tagElements.length < 5) {
        const tagElement = document.createElement("span");
        tagElement.className = "badge btn-primary fs-6 m-1";
        tagElement.textContent = tag;
        tagElement.onclick = function () { this.remove(); updateHiddenTags(); };
        tagContainer.appendChild(tagElement);
        tagInput.value = "";
        updateHiddenTags();
    } else if (tagElements.length >= 5) {
        alert("You can only add up to 5 tags.");
    }

    function updateHiddenTags() {
        let tagsArray = Array.from(tagContainer.getElementsByClassName("badge btn-primary fs-6 m-1")).map(elem => elem.textContent);
        hiddenTags.value = tagsArray.join(',');
    }
});

document.addEventListener("DOMContentLoaded", function () {
    var tagContainer = document.getElementById("tagContainer");
    tagContainer.addEventListener("click", function (event) {
        if (event.target.classList.contains("special-tag")) {
            event.target.remove();
            event.target.style.color = "black";
            updateHiddenTags();
        }
    });
});

let currentStep = 0;
const steps = document.querySelectorAll(".step");
const nextBtn = document.getElementById("nextBtn");
const prevBtn = document.getElementById("prevBtn");
const progressBar = document.getElementById("progressBar");

function updateProgress() {
    const percent = (currentStep / (steps.length - 1)) * 100;
    progressBar.style.width = percent + "%";
}

function showStep(stepIndex) {
    steps.forEach((step, index) => {
        if (index === stepIndex) {
            step.style.display = "block";
        } else {
            step.style.display = "none";
        }
    });
    currentStep = stepIndex;
    updateProgress();

    // Disable/Enable Prev and Next buttons based on current step
    if (currentStep === 0) {
        prevBtn.style.display = "none";
    } else {
        prevBtn.style.display = "inline-block";
    }

    if (currentStep === steps.length - 1) {
        nextBtn.style.display = "none";
    } else {
        nextBtn.style.display = "inline-block";
    }
}

nextBtn.addEventListener("click", () => {
    if (validateStep(currentStep)) {
        if (currentStep < steps.length - 1) {
            showStep(currentStep + 1);
        }
    }
});

prevBtn.addEventListener("click", () => {
    if (currentStep > 0) {
        showStep(currentStep - 1);
    }
});

showStep(0);

function validateStep(step) {
    let isValid = true;
    steps[step].querySelectorAll('[required]').forEach(function (input) {
        if (input.value.trim() === '') {
            isValid = false;
            return false; // exit loop early if any required field is empty
        }
    });
    return isValid;
}

function readURL(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {
            $(".image-upload-wrap").hide();

            $(".file-upload-image").attr("src", e.target.result);
            $(".file-upload-content").show();

            $(".image-title").html(input.files[0].name);
        };

        reader.readAsDataURL(input.files[0]);
    } else {
        removeUpload();
    }
}

function removeUpload() {
    $(".file-upload-input").replaceWith($(".file-upload-input").clone());
    $(".file-upload-content").hide();
    $(".image-upload-wrap").show();
}

$(".image-upload-wrap").bind("dragover", function () {
    $(".image-upload-wrap").addClass("image-dropping");
});

$(".image-upload-wrap").bind("dragleave", function () {
    $(".image-upload-wrap").removeClass("image-dropping");
});

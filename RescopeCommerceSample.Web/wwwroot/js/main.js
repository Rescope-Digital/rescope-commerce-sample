// initialise nice selects
document.querySelectorAll(".ns").forEach((x) => {
  NiceSelect.bind(x, { searchable: true });
  x.style.display = "none";
});

// listen to the 'add to basket' buttons
document.querySelectorAll(".btn_add").forEach((x) => {
  x.addEventListener("click", () => {
    const input = x.parentNode.querySelector("input");
    input.value = Math.min(input.max, parseInt(input.value) + 1);
    var evt = document.createEvent("HTMLEvents");
    evt.initEvent("change", false, true);
    input.dispatchEvent(evt);
  });
});

// submit locale dropdown form when selection changed
document.querySelectorAll("select.countryChanger").forEach((x) => {
  x.addEventListener("change", () => {
    x.closest("form").submit();
  });
});

// listen to the (-) button for product quantity.
document.querySelectorAll(".btn_negate").forEach((x) => {
  x.addEventListener("click", () => {
    const input = x.parentNode.querySelector("input");
    input.value = Math.max(input.min, parseInt(input.value) - 1);
    var evt = document.createEvent("HTMLEvents");
    evt.initEvent("change", false, true);
    input.dispatchEvent(evt);
  });
});

let currentPopup = 0;
const openPopup = (name, subtext, img) => {
  const myId = ++currentPopup;

  const addedToCartPopup = document.getElementById("addedToCartPopup");
  addedToCartPopup.style.maxHeight = "0px";
  addedToCartPopup.classList.remove("hidden");
  addedToCartPopup.classList.add("transition-all", "py-0");
  addedToCartPopup.clientHeight;
  addedToCartPopup.style.maxHeight = "250px";
  addedToCartPopup.classList.remove("py-0");

  addedToCartPopup.querySelector(".noticeImage").src = img;
  addedToCartPopup.querySelector(".noticeProductName").innerText = name;
  addedToCartPopup.querySelector(".noticeProductSubtext").innerText = subtext;

  setTimeout(() => {
    if (currentPopup == myId) {
      addedToCartPopup.style.maxHeight = "0px";
      addedToCartPopup.classList.add("py-0");
      setTimeout(() => {
        if (currentPopup == myId) {
          addedToCartPopup.classList.add("hidden");
        }
      }, 1000);
    }
  }, 5000);
};

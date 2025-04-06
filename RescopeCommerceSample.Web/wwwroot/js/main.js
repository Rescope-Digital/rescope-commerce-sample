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

// display a toast if server sets addedToBasket
if (addedToBasket) {
  Toastify({
    text: "Added to your basket.",
    duration: 3000,
    close: true,
    gravity: "bottom",
    position: "right",
    stopOnFocus: true,
  }).showToast();
}

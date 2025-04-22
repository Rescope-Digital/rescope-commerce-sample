document.querySelectorAll(".quantityinput").forEach((x) => {
  x.addEventListener("change", () => {
    document.querySelector(".update-btn").disabled = false;
    document.querySelector(".checkout-btn").disabled = true;
  });
});

document.querySelector(".checkout-btn").addEventListener("click", (e) => {
  e.preventDefault();
  window.location = document.querySelector(".checkout-btn").dataset.href;
});

const showPasswordIcons = document.querySelectorAll(".show");
showPasswordIcons.forEach(function (showPasswordIcon) {
  showPasswordIcon.addEventListener("click", function () {
    this.classList.toggle("fa-eye-slash");
    const passwordField = this.parentNode.querySelector(".password");
    const type = passwordField.getAttribute("type") === "password" ? "text" : "password";
    passwordField.setAttribute("type", type);
    if (passwordField.type === "text")
      this.style.color = "#376581";
    else
      this.style.color = "#bababa";
  });
});

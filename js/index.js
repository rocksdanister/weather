//threejs scene first run
document.addEventListener("sceneLoaded", () => {
  if (container.style.opacity == 0) setVisible(container);
  $(".indeterminate-progress-bar").css("display", "none");
  $(".item").each(function () {
    $(this).css("background-image", $(this).data("delayedsrc"));
  });
});

//helpers
async function setVisible(element) {
  for (let val = 0; val < 1; val += 0.1) {
    element.style.opacity = val;
    await new Promise((r) => setTimeout(r, 75));
  }
}

function hasClass(element, className) {
  return (" " + element.className + " ").indexOf(" " + className + " ") > -1;
}

function scrollToElement(id) {
  document.getElementById(id).scrollIntoView({ behavior: "smooth", block: "center", inline: "nearest" });
}

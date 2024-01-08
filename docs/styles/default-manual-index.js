var currentPageUrl = window.location.href;
var currentPageFileName = currentPageUrl.substring(currentPageUrl.lastIndexOf("/") + 1);
if (currentPageFileName === "docs/index.html") {
    window.location.href = "../manual/index.html";
}
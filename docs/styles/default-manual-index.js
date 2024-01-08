var currentPageUrl = window.location.href;
var currentPageFileName = currentPageUrl.substring(currentPageUrl.lastIndexOf("/") + 1);
if (currentPageFileName === "USingleton/index.html") {
    window.location.href = "../manual/index.html";
}
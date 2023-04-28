/*!
 * ==========================================
 * Swagger Scrips
 * Use this file to put exclusive swagger scripts
 * ==========================================
 */

$("head title").text("API");
$("#swagger-ui > section > div.topbar > div > div > a > img").hide();
setInterval(function () {
    $("#swagger-ui > section > div.topbar > div > div > a").attr("href", "../");
    $("#swagger-ui > section > div.topbar > div > div > a > img").attr("src", "../favicon.svg").show();
    $("head link[rel='icon']").attr('href', "../favicon.svg");
}, 100);
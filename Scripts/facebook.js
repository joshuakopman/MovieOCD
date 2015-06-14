// You probably don't want to use globals, but this is just example code
var fbAppId = '118454408341531';
// This check is just here to make sure you set your app ID. You don't
// need to use it in production. 
if (fbAppId === 'replace me') {
    alert('Please set the fbAppId in the sample.');
}

// This is boilerplate code that is used to initialize the Facebook
// JS SDK.  You would normally set your App ID in this code.

// Additional JS functions here
window.fbAsyncInit = function () {
    FB.init({
        appId: fbAppId,        // App ID
        status: true,           // check login status
        cookie: true,           // enable cookies to allow the server to access the session
        xfbml: true            // parse page for xfbml or html5 social plugins like login button below
    });

    FB.Event.subscribe('auth.statusChange', function (response) {
        if (response.authResponse != null) {
            fbuserId = response.authResponse.userID;
        }
        if (typeof fbuserId != 'undefined' && fbuserId != null && fbuserId != '' && fbuserId != 0) {
            $(".fb_iframe_widget").hide();
        }
    });
    FB.getLoginStatus(function (response) {
        if (response.status === 'connected') {
            // User logged into FB and authorized
            fbConnected = true;
        }
       /*() if (response.status === 'not_authorized') {
            $(.fb)
        }*/
    });
    // Put additional init code here
};

// Load the SDK Asynchronously
(function (d, s, id) {
    var js, fjs = d.getElementsByTagName(s)[0];
    if (d.getElementById(id)) { return; }
    js = d.createElement(s); js.id = id;
    js.src = "//connect.facebook.net/en_US/all.js";
    fjs.parentNode.insertBefore(js, fjs);
} (document, 'script', 'facebook-jssdk'));
/*
* This function makes a call to the og.likes API.  The object argument is
* the object you like.  Other types of APIs may take other arguments.
* (i.e. the book.reads API takes a book= argument.)
*
* Because it's a sample, it also sets the privacy parameter so that it will
* create a story that only you can see.  Remove the privacy parameter and
* the story will be visible to whatever the default privacy was when you
* added the app.
*
* Also note that you can view any story with the id, as demonstrated with
* the code below.
*
* APIs used in postLik(e)a:
* Call the Graph API from JS:
*   https://developers.facebook.com/docs/reference/javascript/FB.api
* The Open Graph og.likes API:
*   https://developers.facebook.com/docs/reference/opengraph/action-type/og.likes
* Privacy argument:
*   https://developers.facebook.com/docs/reference/api/privacy-parameter
*/

function makeTinyUrl(url) {
    $.getJSON('http://json-tinyurl.appspot.com/?url=' + url + '&callback=?',
    	function (data) {
    	    alert(data.tinyurl);
    	    return data.tinyurl;
    	}
    );
}


function postLike(movieName, rating, movieImagePath,summary) {
  var roundedRating = Math.round(rating);
  FB.api(
       '/me/video.rates',
       'post',
       {  
           'movie': 'http://www.movieocd.com/opengraph/' + movieName + '?movieImagePath=' + encodeURIComponent(movieImagePath)+'&summary='+encodeURIComponent(summary),
           'rating:value': roundedRating,
           'rating:scale': 5,
           'rating:normalized_value': roundedRating / 5,
           'fb:explicitly_shared':true
       },
       function (response) {
           if (!response) {
               console.log('Error occurred.');
           } else if (response.error) {
               console.log(response.error.message);
           } else {
               console.log('shared successfully');
           }
       }
    );
}
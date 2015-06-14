var map;
var infowindow;
var $mapPanel = $(".map-panel");
var $placesList= $("#places");
var mapCanvasDiv = document.getElementById("map-canvas");
var $rangeDropDown = $("#ddlRange");
var $zipCodeTextBox = $("#txtZipCode");
var $findDentistButton = $("#btnFindDentist");

$(document).ready(function(){
	$findDentistButton.bind({
		click: function () {
			submitZip($zipCodeTextBox.val(),$rangeDropDown.val(),$rangeDropDown.find(":selected").attr("meters"));
		}
	});
});

$(document).keypress(function(e) {
    if(e.which == 13) {
        submitZip($zipCodeTextBox.val(),$rangeDropDown.val(),$rangeDropDown.find(":selected").attr("meters"));
    }
});

function submitZip(zipCodeValue,rangeValue,rangeInMeters)
{
	$placesList.empty();
	$mapPanel.slideDown(500,displayMap(zipCodeValue,rangeValue,rangeInMeters));
}
function displayMap(zip,zoom,rangeInMeters)
{
	$.getJSON('http://maps.googleapis.com/maps/api/geocode/json?address='+zip+'&sensor=false', function(data)
	{	
	   var lat = data.results[0].geometry.location.lat;
	   var lng = data.results[0].geometry.location.lng;
	   var userlocation= new google.maps.LatLng(lat,lng);
	   var mapOptions = {
		  center: userlocation,
		  zoom: parseInt(zoom)
	   };
	   
	  map = new google.maps.Map(mapCanvasDiv,mapOptions);

	  var request = {
		radius: parseInt(rangeInMeters),
		location: userlocation,
		types: ['dentist']
	  };
	  infowindow = new google.maps.InfoWindow();
	  var service = new google.maps.places.PlacesService(map);
	  service.nearbySearch(request, callback);

	});

}

function callback(results, status) {
  if (status == google.maps.places.PlacesServiceStatus.OK) {
    for (var i = 0; i < 5; i++) {
      createMarker(results[i]);
    }
  }
}

function createMarker(place) {
  var placeLoc = place.geometry.location;
  var marker = new google.maps.Marker({
    map: map,
    title: place.name,
    position: place.geometry.location
  });

   $placesList.append('<li>' + place.name + '</li>').show();
   google.maps.event.addListener(marker, 'click', function() {
    infowindow.setContent(place.name);
    infowindow.open(map, this);
  });
}

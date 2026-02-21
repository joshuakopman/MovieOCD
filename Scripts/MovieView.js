(function ($) {
    //Models
    RatingProvider = Backbone.Model.extend({
        defaults: {
            Rating: '',
            MaxRating: '',
            LogoUrl: '',
            Status: '',
            Url: ''
        },
        initialize: function () {
            this.Rating = '';
            this.MaxRating = '';
            this.LogoUrl = '';
            this.Status = '';
            this.Url = '';
        }
    });
   
    SuggestedTitle = Backbone.Model.extend({
        defaults: {
            Title: '',
            Year: '',
        },
        initialize: function () {
            this.Title = '';
            this.Year = '';
        }
    });
    
    EditorPick = Backbone.Model.extend({
        defaults: {
            EditorName: ''
        },
        initialize: function () {
            this.EditorName = '';
        }
    });
    
   HistoryItem = Backbone.Model.extend({
        defaults: {
            Title: ''
        },
        initialize: function () {
            this.Title ='';
            this.Year = '';
        }
    });
    
    //Collections
    var Providers = Backbone.Collection.extend({
         model: RatingProvider
    });
    
    var SuggestedTitles = Backbone.Collection.extend({
         model: SuggestedTitle
    });
    
    var EditorPicks = Backbone.Collection.extend({
         model: EditorPick
    });
    
    var HistoryItems = Backbone.Collection.extend({
         model: HistoryItem
    });
    //Views
    var HistoryView = Backbone.View.extend({
        el: $('body'),
        initialize: function () {
            _.bindAll(this, 'render');
            this.render();
        }, 
          render: function (){
                this.displaySearchHistory();
          },
          displaySearchHistory: function() {
            $.each(this.options.historyitemCollection.models, function(key, val) {
                 var yearFormatted='';
                 if (val.Year != '') {
                    yearFormatted = '(' + val.Year + ')';
                 }
                var itemProperties = { Title: val.Title, Year: val.Year, displayYear : yearFormatted};
                if (val.Title != '') {
                    var existingMovie = $(".historyTitle[year='" + val.Year + "'][movietitle='" + val.Title+"']").last();
                    existingMovie.next().remove();
                    existingMovie.remove();
                    $("#historyList").after(_.template(historyItemTemplate, itemProperties));
                    var firstItemTitle = $(".historyTitle").first();
                    firstItemTitle.after("<div class='historyRatings'></div>");
                    $.each(val.Ratings.models, function(key1, val1) {
                        if (val1["Rating"]  != 'N/A' && val1["Rating"]  != '-1')
                        {
                            var ratingproperties = new Object();
                            ratingproperties.ImagePath = val1["LogoUrl"];
                            ratingproperties.Rating = val1["Rating"] + " / " + val1["MaxRating"];
                            firstItemTitle.next().append(_.template(ratingHistoryItemTemplate, ratingproperties));
                        }
                    });
                }
            }); 
        }
    });
    
    var SearchView = Backbone.View.extend({
        el: $('body'),

        initialize: function () {
            _.bindAll(this, 'render');
	        this.MyKonamiArray = new Array();
	        this.KonamiIndex = 0;
            this.providercollection = new Providers();
            this.suggtitlecollection = new SuggestedTitles();
            this.editorpickcollection = new EditorPicks();
            this.historyitemcollection = new HistoryItems();
            this.movieID = '';
            this.summary = '';	
            this.render();
            
        },

        events: {
            'keyup #txtMovieName': 'handleKeyboard',
            'click #btnSubmit': 'searchForTitle',
            'click .suggTitle': 'handleSuggestedTitles',
            'click #btnSearchHistory': 'clearpastsearches',
            'click #displaySearchHistory': 'flipResults',
            'click .historyTitle':'handleSuggestedTitles',
            'hover #movieImage' : 'rateMovie',
            'rated #stars':'saveRating',
            'click #fb-logout':'facebooklogout',
	        'keyup' : 'CheckForKonamiCode'
        },
	    CheckForKonamiCode : function(ev)
	    {
          var konamiCodeArray = new Array(38,38,40,40,37,39,37,39,66,65);
	       this.MyKonamiArray.push(ev.keyCode);
	       if(konamiCodeArray[this.KonamiIndex] == this.MyKonamiArray[this.KonamiIndex])
	       {
		    this.KonamiIndex++;
		    if(konamiCodeArray.length == this.MyKonamiArray.length)
		    {
		       this.titleSearch('Contra', '', this.DisplayResults);
		    }
	       }
	       else
	       {
	  	     this.MyKonamiArray = [];
		     this.KonamiIndex = 0;
	       }

	    },
        render: function () {
            this.repaintView();
        },
        facebooklogout: function() {
            if (window.FB && window.location.protocol === 'https:') {
                FB.logout();
            }
        },
        rateMovie: function(ev) {
                if (!window.FB || window.location.protocol !== 'https:') {
                    $("#stars,#rateMyMovie,#rateMyMovieText,.ratingValue,.fb_iframe_widget,#fb-logout,#chkfacebookShareDiv").hide();
                    return;
                }
                var self = this;
                FB.getLoginStatus(function (response) {
                if (response.status === 'connected') {
                    $('#fb-logout').css('position', 'absolute').show();
                    $("#chkfacebookShareDiv").css('position', 'absolute').show();
                    $("#stars,.ratingValue").show();
                }
                else {
                    $('.fb_iframe_widget').css({ 'position': 'absolute', 'left': '135px', 'top' : '265px' }).show();
                    if ($(".rateit-selected").attr('style').indexOf('width: 0px') > 0) {
                        $("#stars,.ratingValue").hide();
                    }
                }
                if (ev.originalEvent.type.toString() == 'mouseover' && self.movieID != '' && self.movieID!='N/A') {
                   $("#rateMyMovie,#rateMyMovieText").show();
                } else {
                   $("#stars,#rateMyMovie,#rateMyMovieText,.ratingValue,.fb_iframe_widget,#fb-logout,#chkfacebookShareDiv").hide();
                }
              });
        },
        saveRating: function(event, value) {
            if ($("#chkfacebookShare").attr('checked') && window.FB && window.location.protocol === 'https:') {
                postLike(currentMovieName, value, currentMovieImagePath,this.summary);
            }
            var request  = new Object();
            request.userRatingRequest = new Object();
            request.userRatingRequest.IMDBID = this.movieID;
            request.userRatingRequest.Rating = value.toString();
            request.userRatingRequest.UserID = fbuserId;
            if (fbuserId !='' && this.movieID != '' && this.movieID != 'N/A') {
                $.ajax({
                    url: 'api/user/',
                    type: 'POST',
                    dataType: 'json',
                    data: JSON.stringify(request),
                    contentType: "application/json; charset=utf-8"
                }).done(function(data) {
                    $('#value5').text('You\'ve rated it: ' + value + ' stars');
                });
            }
        },
        flipResults:function() {
            var _this = this;
            var mainContent = $(".mainContent");
            var displaySearchHistory = $("#displaySearchHistory");
            var searchHistory = $("#searchHistory");
            if (displaySearchHistory.html() == 'View Recent Searches') {
                mainContent.flip({
                    direction: 'lr',
                    onEnd: function() {
                        displaySearchHistory.html('Back To Movie Results');
                        $("#movieResult").hide();
                        $("h1").show();
                        searchHistory.html('<div id="historyList"></div>').show();
                        var historyView = new HistoryView({ historyitemCollection: _this.historyitemcollection });
                    },
                    color: 'lightgrey'
                });
            } else {
                mainContent.flip({
                    direction: 'rl',
                    onEnd: function() {
                        displaySearchHistory.html('View Recent Searches');
                        $("#movieResult").show();
                        $("h1").hide();
                        searchHistory.hide();
                    },
                    color: 'lightgrey'
                });
            }
        },
        repaintView: function () {
            this.providercollection = new Providers();
            this.suggtitlecollection = new SuggestedTitles();
            this.editorpickcollection = new EditorPicks();
            $('#results,#foundTitle,#suggestedTitles,#pastSearches').html('');
            $('#bottomSearch,.fb_iframe_widget,h1').hide();
            $("#movieImg").fadeOut(200);
             $( document ).tooltip({
                 position: {
                     my: "center top",
                     at: "center-20 bottom",
                     using: function(position, feedback) {
                         $(this).css(position);
                         $(this).addClass('toolTip');
                     }
                 }
             });
            $("#txtMovieName").removeAttr('disabled').focus(function () {
            if (this.value == "Search Titles") {
                    $(this).val("");
                }
            }).blur(function () {
                if (this.value == "") {
                    $(this).val("Search Titles");
                }
            });
        },
        searchForTitle: function () {
            var textbox = $("#txtMovieName");
            if (textbox.val() != "" && textbox.val() != "Search Titles") {
                this.titleSearch(textbox.val(), '', this.DisplayResults);
                textbox.attr('disabled', 'disabled');
            }
        },

        clearpastsearches: function () {
            $('#pastSearches').html('');
        },
        getUserRating: function() {
          var savedrating = '';
          if (fbuserId !='' && this.movieID != '' && this.movieID != 'N/A') {
                $.ajax({
                    url: 'api/User?userid=' + fbuserId + '&imdbid=' + this.movieID,
                    type: 'GET',
                    contentType: "application/json; charset=utf-8",
                }).done(function(data) {
                    savedrating = data.Rating;
                    if (savedrating != null) {
                        $('#value5').text('You\'ve rated it: ' + savedrating + ' stars');
                        $('#stars').rateit('value', savedrating);
                    }
                }); 
            }
        },
        handleKeyboard: function (event) {
            if (event.keyCode == 13) {
                $("#btnSubmit").click();
            }
        },

        handleSuggestedTitles: function (ev) {
            $("#displaySearchHistory").html('View Recent Searches');
            var selectedSuggTitle = ev.currentTarget;
            $("#suggestedTitles").html('');
            $("h1").hide();
            this.titleSearch($(selectedSuggTitle).attr('movietitle'), $(selectedSuggTitle).attr('year'), this.DisplayResults);
        },

        DisplayResults: function(imagepath, collectionreference) {
            var resultsElement = $('#results');
            var suggTitlesElement = $("#suggestedTitles");
            var foundTitleElement = $('#foundTitle');
            var bottomSearchElement = $("#bottomSearch");
            var searchTextBox = $('#txtMovieName');
            var poster = $('#movieImg');
            var suggestedTitleHeader = $("#suggheader");
            var suggtitles = collectionreference.suggtitlecollection.models;
            var ratProviders = collectionreference.providercollection.models; 
            var editorPicks = collectionreference.editorpickcollection.models;
            if (typeof suggtitles == 'undefined') {
                suggestedTitleHeader.hide();
            }
            $.each(ratProviders, function(key, val) {
                if (val.Rating != 'N/A' && val.Rating != '-1' ) {
                    var imgProperties = {url: val.Url, logourl: val.LogoUrl, rating: val.Rating, maxrating: val.MaxRating };
                    resultsElement.append(_.template(ratingProvidersTemplate, imgProperties));
                }
            });

            $.each(suggtitles, function(key, val) {
                            var yearFormatted='';
            if (val.Year != '') {
                yearFormatted = '(' + val.Year + ')';
            }
                var titleProperties = { title: val.Title, year: val.Year, displayYear : yearFormatted};
                suggTitlesElement.append(_.template(titlesTemplate, titleProperties));
            });
            
            if (editorPicks != null && editorPicks.length > 0) {
                $.each(editorPicks, function(key, val) {
                    var editorProperties = { EditorName: val.EditorName };
                    resultsElement.append(_.template(editorTemplate, editorProperties));
                });
            if (editorPicks.length > 1) {
                    $(".editorPick").css({ 'margin-left': '28px' });
                }
            }
            if (imagepath != '') {
                poster.fadeIn(200);
                poster.attr('src', imagepath);
                currentMovieImagePath = imagepath;
            }
            if (typeof ratProviders[0] !== 'undefined' && (ratProviders[0].Status == 1 || ratProviders[1].Status == 1 || ratProviders[2].Status == 1)) {
                resultsElement.show();
                foundTitleElement.show();
            }

            bottomSearchElement.show();
            suggTitlesElement.show();
          //  searchTextBox.val('');
          //  searchTextBox.blur();
        },
        titleSearch: function (title, year, resultsCallback) {
            var self = this;
            var foundTitle = $('#foundTitle');
            var progress = $("#progressbar");
            var suggTitles = $("#suggestedTitles");
            var searchBox = $("#txtMovieName");
            var myUrl;
            
            if (typeof year !== 'undefined' && year != '') {
                myUrl = '/movie/search/' + encodeURIComponent(title).replace(/\./g,'-').replace(/\%2B/g,'-').replace('%2F','-') + "/" + year;
            } else {
                myUrl = '/movie/search/' + encodeURIComponent(title).replace(/\./g,'-').replace(/\%2B/g,'-').replace('%2F','-');
            }
            $("#movieResult,#displaySearchHistory,#divider").show();
            progress.show();
            $("#searchHistory,#displaySearchHistory").hide();
            $('.ratingValue').html('');
            $("#btnSubmit").attr('disabled', 'disabled');
            $('#stars').rateit('value', null); 
            $.ajax({
                url: myUrl,
                type: 'GET',
                dataType: 'json',
                beforeSend: function(xhr) {
                    xhr.setRequestHeader('APIKey', 'McLaineInTheMembrane');
                }
            }).done(function(data) {
                var historyItem = new HistoryItem();
                if (data.Title != '') {
                    document.title = 'MovieOCD - ' + data.Title;
                    var yearFormatted='';
                    if (data.Year != '') {
                        yearFormatted = '(' + data.Year + ')';
                    }
                    var titleProperties = { title: data.Title, year: data.Year, displayYear : yearFormatted };
                    foundTitle.append(_.template(foundTitleTemplate, titleProperties));
                    currentMovieName = data.Title;
                }
                if (data.PlotSummary != 'N/A') {
                    $("#titleResult").attr('title', data.PlotSummary);
                    self.summary = data.PlotSummary;
                }
                $.each(data.RatingProviders, function(key, val) {
                    if (val.Status == '1') {
                        if (val.DisplayName == 'IMDB') {
                            self.movieID = val.ID;
                        }
                        var provider = new RatingProvider();
                        provider.Rating = val.Rating;
                        provider.MaxRating = val.MaxRating,
                        provider.LogoUrl= val.LogoUrl;
                        provider.Status = val.Status;
                        provider.Url = 'http://www.'+val.DisplayName.replace(' ', '')+'.com'; 
                        self.providercollection.add(provider);
                    }
                });

                $.each(data.SuggestedTitles, function(key, val) {
                    var suggtitle = new SuggestedTitle();
                    suggtitle.Title = val.Title;
                    suggtitle.Year = val.Year;
                    self.suggtitlecollection.add(suggtitle);
                });
                
                if (data.EditorPicks != null && data.EditorPicks.length > 0) {
                    $.each(data.EditorPicks, function(key, val) {
                        var editorpick = new EditorPick();
                        editorpick.EditorName = val.EditorName;
                        self.editorpickcollection.add(editorpick);
                    });
                }
                historyItem.Title = data.Title;
                historyItem.Year = data.Year;
                historyItem.Ratings = self.providercollection;
                self.historyitemcollection.add(historyItem);
                self.getUserRating();
                resultsCallback(data.LargeImagePath, self);


            }).fail(function() {
                suggTitles.append("Unable to find any matching results. Please try another title.");
                suggTitles.show();
                $("#bottomSearch").show();
                $("#suggheader").hide();
            }).always(function() {
                $("#displaySearchHistory").html('View Recent Searches').show();
                searchBox.removeAttr('disabled');
                progress.hide();
                $("#divider").show();
                $("#btnSubmit").removeAttr('disabled', 'disabled');
            });
            
            this.repaintView();
        },
    });

    var searchView = new SearchView();
})(jQuery);

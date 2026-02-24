(function ($) {
    var HISTORY_KEY = 'movieocd_recent_searches';
    var MAX_HISTORY = 8;
    var demoRatings = {};
    var currentMovieKey = '';
    var isFlipping = false;

    var MOCK_MOVIES = [
        {
            title: 'Pulp Fiction',
            year: '1994',
            director: 'Quentin Tarantino',
            imagePath: 'Images/pulp-fiction-poster.jpg',
            ratings: [
                { displayName: 'IMDB', rating: '8.9', maxRating: '10', logoUrl: 'Images/imdb.jpg', url: 'https://www.imdb.com/title/tt0110912/' },
                { displayName: 'Netflix', rating: '4.5', maxRating: '5', logoUrl: 'Images/netflix.jpg', url: 'https://www.netflix.com/' },
                { displayName: 'Rotten Tomatoes', rating: '92', maxRating: '100', logoUrl: 'Images/tomatoes.jpg', url: 'https://www.rottentomatoes.com/m/pulp_fiction' }
            ],
            suggestedTitles: [
                { title: 'Reservoir Dogs', year: '1992' },
                { title: 'Jackie Brown', year: '1997' }
            ]
        },
        {
            title: 'Inception',
            year: '2010',
            director: 'Christopher Nolan',
            imagePath: 'Images/inception-poster.jpg',
            ratings: [
                { displayName: 'IMDB', rating: '8.8', maxRating: '10', logoUrl: 'Images/imdb.jpg', url: 'https://www.imdb.com/title/tt1375666/' },
                { displayName: 'Netflix', rating: '4.4', maxRating: '5', logoUrl: 'Images/netflix.jpg', url: 'https://www.netflix.com/' },
                { displayName: 'Rotten Tomatoes', rating: '87', maxRating: '100', logoUrl: 'Images/tomatoes.jpg', url: 'https://www.rottentomatoes.com/m/inception' }
            ],
            suggestedTitles: [
                { title: 'Interstellar', year: '2014' },
                { title: 'Memento', year: '2000' }
            ]
        },
        {
            title: 'The Matrix',
            year: '1999',
            director: 'The Wachowskis',
            imagePath: 'Images/matrix-poster.jpg',
            ratings: [
                { displayName: 'IMDB', rating: '8.7', maxRating: '10', logoUrl: 'Images/imdb.jpg', url: 'https://www.imdb.com/title/tt0133093/' },
                { displayName: 'Netflix', rating: '4.6', maxRating: '5', logoUrl: 'Images/netflix.jpg', url: 'https://www.netflix.com/' },
                { displayName: 'Rotten Tomatoes', rating: '83', maxRating: '100', logoUrl: 'Images/tomatoes.jpg', url: 'https://www.rottentomatoes.com/m/matrix' }
            ],
            suggestedTitles: [
                { title: 'The Matrix Reloaded', year: '2003' },
                { title: 'Dark City', year: '1998' }
            ]
        }
    ];

    function normalize(value) {
        return (value || '').toLowerCase().replace(/[^a-z0-9]+/g, ' ').replace(/^\s+|\s+$/g, '');
    }

    function escapeHtml(value) {
        return (value || '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function cloneMovie(movie) {
        return JSON.parse(JSON.stringify(movie));
    }

    function findMovie(title) {
        var normalizedTitle = normalize(title);
        var i;

        for (i = 0; i < MOCK_MOVIES.length; i++) {
            if (normalize(MOCK_MOVIES[i].title) === normalizedTitle) {
                return cloneMovie(MOCK_MOVIES[i]);
            }
        }

        for (i = 0; i < MOCK_MOVIES.length; i++) {
            if (normalizedTitle && normalize(MOCK_MOVIES[i].title).indexOf(normalizedTitle) >= 0) {
                return cloneMovie(MOCK_MOVIES[i]);
            }
        }

        return null;
    }

    function getRecentSearches() {
        var raw = window.localStorage ? localStorage.getItem(HISTORY_KEY) : null;
        if (!raw) {
            return [];
        }

        try {
            var parsed = JSON.parse(raw);
            return Object.prototype.toString.call(parsed) === '[object Array]' ? parsed : [];
        } catch (err) {
            return [];
        }
    }

    function saveRecentSearch(movie) {
        if (!window.localStorage) {
            return;
        }

        var existing = getRecentSearches();
        var normalizedTitle = normalize(movie.title);
        var i;
        var updated = [movie];

        for (i = 0; i < existing.length; i++) {
            if (normalize(existing[i].title) !== normalizedTitle) {
                updated.push(existing[i]);
            }
        }

        if (updated.length > MAX_HISTORY) {
            updated = updated.slice(0, MAX_HISTORY);
        }

        localStorage.setItem(HISTORY_KEY, JSON.stringify(updated));
    }

    function renderRatings(ratings) {
        var resultsElement = $('#results');
        var resultsDiv = $('#resultsDiv');

        resultsElement.empty();
        resultsElement.append('<div id="resultsDiv"></div>');
        resultsDiv = $('#resultsDiv');

        $.each(ratings, function (_, rating) {
            if (rating.rating !== 'N/A' && rating.rating !== '-1') {
                resultsDiv.append(
                    '<a href="' + escapeHtml(rating.url || '#') + '" target="_blank">' +
                    '<img class="providerLogo" src="' + escapeHtml(rating.logoUrl) + '" alt="' + escapeHtml(rating.displayName) + '"></a>' +
                    '<p class="rating">' + escapeHtml(rating.rating) + '/' + escapeHtml(rating.maxRating) + '</p><br/>'
                );
            }
        });
    }

    function renderSuggestions(suggestedTitles) {
        var suggestedElement = $('#suggestedTitles');
        suggestedElement.empty();

        $.each(suggestedTitles || [], function (_, item) {
            var displayYear = item.year ? ' (' + item.year + ')' : '';
            suggestedElement.append(
                '<div class="suggTitle" movietitle="' + escapeHtml(item.title) + '" year="' + escapeHtml(item.year || '') + '">' +
                escapeHtml(item.title) + displayYear + '</div><br/>'
            );
        });
    }

    function renderMovie(movie, persistHistory) {
        var displayYear = movie.year ? '(' + movie.year + ')' : '';
        currentMovieKey = normalize(movie.title);

        $('#bottomSearch').show();
        $('#movieResult').show();
        $('#searchHistory').hide();
        $('h1').hide();
        $('#displaySearchHistory').html('View Recent Searches').show();

        $('#foundTitle').html(
            '<div class="searchTitle"><div id="titleResult">' + escapeHtml(movie.title) + '</div>' + escapeHtml(displayYear) + ' </div>'
        ).show();

        $('#movieImg').attr('src', movie.imagePath).show();
        renderDemoRatingState();

        renderRatings(movie.ratings);
        renderSuggestions(movie.suggestedTitles);

        if (persistHistory !== false) {
            saveRecentSearch(movie);
        }
    }

    function renderNotFound(title) {
        var fallbackSuggestions = [];
        $.each(MOCK_MOVIES, function (_, movie) {
            fallbackSuggestions.push({ title: movie.title, year: movie.year });
        });

        $('#bottomSearch').show();
        $('#movieResult').show();
        $('#searchHistory').hide();
        $('h1').hide();

        $('#movieImg').hide();
        $('#rateMyMovie,#rateMyMovieText,#stars,.ratingValue').hide();
        $('#results').empty().append('<div id="resultsDiv"></div>');
        $('#foundTitle').html('<div class="searchTitle">Movie Not Found</div>').show();
        renderSuggestions(fallbackSuggestions);

        $('#displaySearchHistory').html('View Recent Searches').show();
        $('#txtMovieName').val(title);
    }

    function renderHistoryView() {
        var history = getRecentSearches();
        var searchHistory = $('#searchHistory');

        searchHistory.html('<div id="historyList"></div>').show();

        if (history.length === 0) {
            searchHistory.append('<div class="historyTitle">No recent searches yet.</div>');
            return;
        }

        $.each(history, function (_, movie) {
            var displayYear = movie.year ? ' (' + movie.year + ')' : '';
            var row = $('<div class="historyTitle" movietitle="' + escapeHtml(movie.title) + '" year="' + escapeHtml(movie.year || '') + '">' +
                escapeHtml(movie.title) + displayYear + '</div>');
            var ratings = $('<div class="historyRatings"></div>');

            $.each(movie.ratings || [], function (_, rating) {
                ratings.append('<img class="thumbnail" src="' + escapeHtml(rating.logoUrl) + '" alt="' + escapeHtml(rating.displayName) + '"> ' +
                    escapeHtml(rating.rating) + ' / ' + escapeHtml(rating.maxRating) + ' ');
            });

            searchHistory.append(row);
            searchHistory.append(ratings);
        });
    }

    function runFlip(direction, onEnd) {
        var mainContent = $('.mainContent');
        var priorPointerEvents = mainContent.css('pointer-events');
        var priorBackground = mainContent.css('background-color');

        if (isFlipping || !$.fn.flip) {
            return;
        }

        isFlipping = true;
        mainContent.css({
            'pointer-events': 'none',
            'background-color': 'lightgrey'
        });

        mainContent.flip({
            direction: direction,
            onEnd: function () {
                try {
                    onEnd();
                } finally {
                    isFlipping = false;
                    mainContent.css({
                        'pointer-events': priorPointerEvents || '',
                        'background-color': priorBackground || ''
                    });
                }
            },
            color: 'lightgrey'
        });
    }

    function flipResults() {
        var displaySearchHistory = $('#displaySearchHistory');
        var searchHistory = $('#searchHistory');

        if (displaySearchHistory.html() === 'View Recent Searches') {
            renderHistoryView();
            searchHistory.hide();

            runFlip('lr', function () {
                displaySearchHistory.html('Back To Movie Results');
                $('#movieResult').hide();
                $('h1').show();
                searchHistory.show();
            });
        } else {
            runFlip('rl', function () {
                displaySearchHistory.html('View Recent Searches');
                $('#movieResult').show();
                $('h1').hide();
                searchHistory.hide();
            });
        }
    }

    function runSearch() {
        var textbox = $('#txtMovieName');
        var query = $.trim(textbox.val());

        if (!query || query === 'Search Titles') {
            return;
        }

        $('#progressbar').show();

        window.setTimeout(function () {
            var movie = findMovie(query);

            if (movie) {
                renderMovie(movie, true);
            } else {
                renderNotFound(query);
            }

            $('#progressbar').hide();
            textbox.blur();
        }, 180);
    }

    function wireEvents() {
        var textbox = $('#txtMovieName');

        $('#btnSubmit').on('click', function (event) {
            event.preventDefault();
            runSearch();
        });

        textbox.on('keypress', function (event) {
            if (event.which === 13) {
                event.preventDefault();
                runSearch();
            }
        });

        textbox.on('focus', function () {
            if ($(this).val() === 'Search Titles') {
                $(this).val('');
            }
        });

        textbox.on('blur', function () {
            if ($.trim($(this).val()) === '') {
                $(this).val('Search Titles');
            }
        });

        $('#displaySearchHistory').on('click', function () {
            if (!isFlipping && $.fn.flip) {
                flipResults();
            }
        });

        $(document).on('click', '.suggTitle', function () {
            var title = $(this).attr('movietitle') || $(this).text();
            $('#txtMovieName').val(title);
            runSearch();
        });

        $(document).on('click', '.historyTitle', function () {
            var title = $(this).attr('movietitle');
            var movie = findMovie(title);
            if (movie) {
                renderMovie(movie, false);
                if ($.fn.flip && $('#displaySearchHistory').html() === 'Back To Movie Results') {
                    flipResults();
                }
            }
        });

        $(document).on('mouseenter', '#movieImage', function () {
            if (!currentMovieKey) {
                return;
            }
            $('#rateMyMovie,#rateMyMovieText,#stars,.ratingValue').show();
        });

        $(document).on('mouseleave', '#movieImage', function () {
            $('#rateMyMovie,#rateMyMovieText,#stars,.ratingValue').hide();
        });

        $('#stars').on('rated', function (_, value) {
            if (!currentMovieKey) {
                return;
            }
            demoRatings[currentMovieKey] = value;
            $('#value5').text('Your demo rating: ' + value + ' stars');
        });
    }

    function renderDemoRatingState() {
        var saved = demoRatings[currentMovieKey];
        $('#stars').rateit('value', saved || null);
        if (saved) {
            $('#value5').text('Your demo rating: ' + saved + ' stars');
        } else {
            $('#value5').text('');
        }
        $('#rateMyMovie,#rateMyMovieText,#stars,.ratingValue').hide();
    }

    $(function () {
        $('#progressbar').hide();
        $('#displaySearchHistory').show();
        wireEvents();
    });
})(jQuery);

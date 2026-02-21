(function () {
    var HISTORY_KEY = 'movieocd_recent_searches';
    var MAX_HISTORY = 8;

    var MOCK_MOVIES = [
        {
            title: 'Pulp Fiction',
            year: '1994',
            director: 'Quentin Tarantino',
            imagePath: 'Images/pulp-fiction-poster.jpg',
            ratings: [
                { displayName: 'IMDB', rating: '8.9', maxRating: '10', logoUrl: 'Images/imdb.jpg' },
                { displayName: 'Netflix', rating: '4.5', maxRating: '5', logoUrl: 'Images/netflix.jpg' },
                { displayName: 'Rotten Tomatoes', rating: '92', maxRating: '100', logoUrl: 'Images/tomatoes.jpg' }
            ],
            suggestedTitles: ['Reservoir Dogs (1992)', 'Jackie Brown (1997)']
        },
        {
            title: 'Inception',
            year: '2010',
            director: 'Christopher Nolan',
            imagePath: 'Images/inception-poster.jpg',
            ratings: [
                { displayName: 'IMDB', rating: '8.8', maxRating: '10', logoUrl: 'Images/imdb.jpg' },
                { displayName: 'Netflix', rating: '4.4', maxRating: '5', logoUrl: 'Images/netflix.jpg' },
                { displayName: 'Rotten Tomatoes', rating: '87', maxRating: '100', logoUrl: 'Images/tomatoes.jpg' }
            ],
            suggestedTitles: ['Interstellar (2014)', 'Memento (2000)']
        },
        {
            title: 'The Matrix',
            year: '1999',
            director: 'The Wachowskis',
            imagePath: 'Images/matrix-poster.jpg',
            ratings: [
                { displayName: 'IMDB', rating: '8.7', maxRating: '10', logoUrl: 'Images/imdb.jpg' },
                { displayName: 'Netflix', rating: '4.6', maxRating: '5', logoUrl: 'Images/netflix.jpg' },
                { displayName: 'Rotten Tomatoes', rating: '83', maxRating: '100', logoUrl: 'Images/tomatoes.jpg' }
            ],
            suggestedTitles: ['The Matrix Reloaded (2003)', 'Dark City (1998)']
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

    function findMovie(title) {
        var normalizedTitle = normalize(title);
        var i;

        for (i = 0; i < MOCK_MOVIES.length; i++) {
            if (normalize(MOCK_MOVIES[i].title) === normalizedTitle) {
                return MOCK_MOVIES[i];
            }
        }

        for (i = 0; i < MOCK_MOVIES.length; i++) {
            if (normalize(MOCK_MOVIES[i].title).indexOf(normalizedTitle) >= 0 && normalizedTitle.length > 0) {
                return MOCK_MOVIES[i];
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

    function renderHistory() {
        var history = getRecentSearches();
        var historyElement = $('#searchHistory');
        var titleElement = $('h1');

        historyElement.empty();

        if (history.length === 0) {
            historyElement.append('<div class="historyTitle">No recent searches yet.</div>');
        }

        $.each(history, function (_, movie) {
            var row = $('<div class="historyTitle"></div>');
            row.text(movie.title + ' (' + movie.year + ')');
            row.click(function () {
                renderMovie(movie, false);
            });
            historyElement.append(row);
        });

        titleElement.show();
        historyElement.show();
    }

    function renderRatings(ratings) {
        var resultsDiv = $('#resultsDiv');
        resultsDiv.empty();

        $.each(ratings, function (_, rating) {
            var row = $('<div class="rating"></div>');
            var logo = $('<img class="providerLogo" alt="Provider logo">');
            logo.attr('src', rating.logoUrl);

            row.append(logo);
            row.append('&nbsp;&nbsp;&nbsp;' + escapeHtml(rating.rating + '/' + rating.maxRating));
            resultsDiv.append(row);
        });
    }

    function renderSuggestions(suggestedTitles) {
        var suggestedElement = $('#suggestedTitles');
        suggestedElement.empty();

        $.each(suggestedTitles || [], function (_, title) {
            suggestedElement.append('<div class="suggTitle">' + escapeHtml(title) + '</div>');
        });
    }

    function renderMovie(movie, persistHistory) {
        $('#progressbar').hide();
        $('#displaySearchHistory').show();
        $('#bottomSearch').show();
        $('#searchHistory').hide();
        $('h1').hide();

        $('#movieImg').attr('src', movie.imagePath).show();

        $('#foundTitle').html(
            '<div class="searchTitle">' +
            escapeHtml(movie.title) +
            '<span style="font-size:68px;color:grey;"><br/>(' + escapeHtml(movie.year) + ')</span></div>'
        ).show();

        renderRatings(movie.ratings);
        renderSuggestions(movie.suggestedTitles);

        if (persistHistory !== false) {
            saveRecentSearch(movie);
        }
    }

    function renderNotFound(title) {
        $('#progressbar').hide();
        $('#displaySearchHistory').show();
        $('#bottomSearch').show();

        $('#movieImg').hide();
        $('#resultsDiv').empty();

        $('#foundTitle').html(
            '<div class="searchTitle">Movie Not Found<span style="font-size:40px;color:grey;"><br/>' +
            escapeHtml(title) +
            '</span></div>'
        ).show();

        var fallbackSuggestions = [];
        $.each(MOCK_MOVIES, function (_, movie) {
            fallbackSuggestions.push(movie.title + ' (' + movie.year + ')');
        });

        renderSuggestions(fallbackSuggestions);
    }

    function searchForTitle() {
        var textbox = $('#txtMovieName');
        var title = $.trim(textbox.val());

        if (!title || title === 'Search Titles') {
            return;
        }

        $('#progressbar').show();

        window.setTimeout(function () {
            var movie = findMovie(title);
            if (movie) {
                renderMovie(movie, true);
            } else {
                renderNotFound(title);
            }
            textbox.blur();
        }, 180);
    }

    function wireEvents() {
        var textbox = $('#txtMovieName');

        $('#btnSubmit').click(function (event) {
            event.preventDefault();
            searchForTitle();
        });

        textbox.keypress(function (event) {
            if (event.which === 13) {
                event.preventDefault();
                searchForTitle();
            }
        });

        textbox.focus(function () {
            if ($(this).val() === 'Search Titles') {
                $(this).val('');
            }
        });

        textbox.blur(function () {
            if ($.trim($(this).val()) === '') {
                $(this).val('Search Titles');
            }
        });

        $('#displaySearchHistory').click(function () {
            renderHistory();
        });
    }

    $(function () {
        wireEvents();
    });
})();

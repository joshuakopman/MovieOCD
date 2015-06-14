ratingProvidersTemplate = '<a href="<%=url%>" target="_blank"><img class="providerLogo" src="<%=logourl %>"></img></a><p class="rating"><%=rating %>/<%=maxrating %></p><br/></div>';
titlesTemplate = '<div class="suggTitle" movietitle="<%= title%>" year="<%= year%>"> <%= title%> <%= displayYear%> </div><br/>';
foundTitleTemplate = '<div class="searchTitle"><div id="titleResult"><%= title %></div><%=displayYear%> </div>';
editorTemplate = '<img class="editorPick" src="Images/<%=EditorName %>.png">';
historyItemTemplate = '<div class="historyTitle" movietitle="<%= Title%>" year="<%= Year%>"><%= Title %> <%=displayYear%></div>';
ratingHistoryItemTemplate = '<img class="thumbnail" src="<%= ImagePath %>"> <%=Rating %>   ';
var fbuserId = '';
var fbConnected = false;
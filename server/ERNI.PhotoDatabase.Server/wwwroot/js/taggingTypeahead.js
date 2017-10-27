function initializeTagging(url) {
    var tags = new Bloodhound({
        datumTokenizer: Bloodhound.tokenizers.whitespace,
        queryTokenizer: Bloodhound.tokenizers.whitespace,
        remote: {
            url: url + '?query=%QUERY',
            wildcard: '%QUERY'
        }
    });

    tags.initialize();

    $('.tag-input').tagsinput({
        typeaheadjs: {
            source: tags.ttAdapter()
        }
    });
}


﻿
describe('TartanRugCellBuilder', function () {

    // Set global
    FT.url = { img: '' };


    var rowNumber = 3,
        columnNumber = 5,
        comparatorId = 4,
        val = 2.2,
        valF = '3.3';

    var getHtml = function (parameters) {

        var data = {
            Val: val,
            ValF: valF,
            Sig: {}
        };

        var useRag = false;

        if (parameters) {

            var sig = parameters.sig;
            if (_.isNumber(sig)) {
                data.Sig[comparatorId] = sig;
            }

            if (parameters.useRag) {
                useRag = parameters.useRag;
            }

            if (parameters.noteId) {
                data.NoteId = parameters.noteId;
            }

            if (parameters.val) {
                data.Val = parameters.val;
            }

            if (parameters.valF) {
                data.ValF = parameters.valF;
            }
        }
        var comparisonConfig = { useRagColours: useRag, comparatorId: comparatorId };

        return new TartanRugCellBuilder(data, columnNumber, rowNumber,
            comparisonConfig).getHtml();
    };

    it('validate generated HTML', function () {
        new HtmlChecker(getHtml({ sig: 0 })).validate();
    });


    it('RAG/BOB none class is assigned correctly', function () {
        var html = getHtml({ sig: 0 });
        expect($(html)).toBeMatchedBy('td.none');
    });

    it('BOB same class is assigned correctly', function () {
        var html = getHtml({ useRag: false, sig: 2 });
        expect($(html)).toBeMatchedBy('td.same');
    });

    it('BOB higher class is assigned correctly', function () {
        var html = getHtml({ useRag: false, sig: 3 });
        expect($(html)).toBeMatchedBy('td.bobHigher');
    });

    it('BOB worse class is assigned correctly', function () {
        var html = getHtml({ useRag: false, sig: 1 });
        expect($(html)).toBeMatchedBy('td.bobLower');
    });

    it('RAG same class is assigned correctly', function () {
        var html = getHtml({ useRag: true, sig: 2 });
        expect($(html)).toBeMatchedBy('td.same');
    });

    it('RAG better class is assigned correctly', function () {
        var html = getHtml({ useRag: true, sig: 3 });
        expect($(html)).toBeMatchedBy('td.better');
    });

    it('RAG worse class is assigned correctly', function () {
        var html = getHtml({ useRag: true, sig: 1 });
        expect($(html)).toBeMatchedBy('td.worse');
    });

    it('useRag flag true displays RAG', function () {
        var html = getHtml({ useRag: true, sig: 1 });
        expect($(html)).toBeMatchedBy('td.worse');
        expect($(html)).not.toBeMatchedBy('td.bobLower');
    });

    it('useRag flag false displays BOB', function () {
        var html = getHtml({ useRag: false, sig: 1 });
        expect($(html)).toBeMatchedBy('td.bobLower');
        expect($(html)).not.toBeMatchedBy('td.worse');
    });

    it('ID contains column number and row number', function () {
        expect($(getHtml())).toBeMatchedBy('#tc-5-3');
    });

    it('Value is displayed if defined', function () {
        expect(getHtml()).toContain(valF);
    });

    it('Note symbol and value are displayed if NoteId defined and Val defined', function () {
        var html = getHtml({ noteId: 100 });
        expect(html).toContain(VALUE_NOTE);
        expect(html).toContain(valF);
    });

    it('Only note symbol is displayed if NoteId defined and Val not defined', function () {
        var html = getHtml({ noteId: 100, val: -1, valF: '-' });
        expect(html).toContain(VALUE_NOTE);
    });
});
rankingsState = null;
function initPage() {
    showLoadingSpinner();
    tooltipManager.init();
    updateModelFromHash();
    setSearchText(MT.model.parentAreaType);
}

function updatePage() {
    resetRankingsState();

    var model = MT.model;

    setGlobalGroupIds();

    ajaxMonitor.setCalls(9);

    getAreaAddress(model.parentCode);
    getIndicatorMetadata(model.groupId, GET_METADATA_DEFINITION);
    getIndicatorMetadata(selectedSupportingGroupId);
    getGroupRoots(model);
    getSupportingGroupRoots(model);
    getSupportingAreaDetails(model.parentCode, model.parentAreaType);
    getAreaTypes();
    getEnglandPrimaryData(model);
    getEnglandSupportingData(model);
    ajaxMonitor.monitor(getSecondaryData);
}

function setGlobalGroupIds() {
    var model = MT.model;
    var startingGroupId = model.groupId;

    var indexOfGroupId = _.indexOf(groupIds, startingGroupId);
    isSupportingIndicatorSelected = indexOfGroupId >= domainsToDisplay;

    // Set supporting group id
    selectedSupportingGroupId = isSupportingIndicatorSelected
        ? groupIds[indexOfGroupId]
        : groupIds[domainsToDisplay];

    // Set primary group id
    model.groupId = isSupportingIndicatorSelected
        ? groupIds[0] // group id on model is of a supporting domain
        : model.groupId;
}

function getSupportingDataValues(prevAndRiskRootIndex) {
    var root = supportingGroupRoots[prevAndRiskRootIndex];
    var groupId = selectedSupportingGroupId;

    var parameters = new ParameterBuilder();
    parameters.add('par', MT.model.parentCode);
    parameters.add('gid', groupId);
    parameters.add('ati', MT.model.areaTypeId);
    parameters.add('off', 0);
    parameters.add('iid', root.IID);
    parameters.add('age', root.AgeId);
    parameters.add('sex', root.SexId);
    parameters.add('pid', MT.model.profileId);

    getData(function (obj) {

        loaded.supportingDataValues[MT.model.areaTypeId] = obj;

        ajaxMonitor.callCompleted();
    }, 'av', parameters.build());
};

function getSecondaryData() {
    changeNumberOrder();   // Reorder table_options pretext

    var model = MT.model;
    setGlobalIndicatorIds();

    rankingsState.comparisonIsAvailable = isComparisonAvailable(model);
    rankingsState.compareSimilar = isSimilarAreas();

    ajaxMonitor.setCalls(rankingsState.comparisonIsAvailable ? 6 : 5);

    if (rankingsState.comparisonIsAvailable) {
        getComparisonValues(model);
    }

    loaded.primaryDataValues.fetchDataByAjax(groupRoots[selectedRootIndex]);
    loaded.practiceValues.fetchDataByAjax(groupRoots[ROOT_INDEXES.POPULATION]);
    getPracticeList();
    getSupportingDataValues(selectedSupportingGroupRootIndex);

    getValueNotes();

    ajaxMonitor.monitor(displayPage);

    populateAreaTypes(model);
    populateCauseList();
    createExportLinks();
    populateSupportingIndicatorList();
    removeLoadingSpinner();
}

function getComparisonValues(model) {
    switch (model.areaTypeId) {
        case AreaTypeIds.CountyUA:
            getDecileData(model);
            break;
        case PRACTICE:
            break;
        case AreaTypeIds.CCG:
            getDecileData(model);
            break;
    }
}

function resetRankingsState() {

    // Preserve table ordering
    var state = rankingsState;
    var sortAscending = state ? state.sortAscending : true;
    var sortedColumn = state ? state.sortedColumn : 2;

    // Global variable containing state specific to this page
    rankingsState = {
        // Row objects that are fed into a template for generating table row HTML
        rows: [],
        // Key(area code)/value(a row) pair list to enable easy look up of a row
        areaCodeToRowHash: {},
        // Whether the view has been initialised
        isInitialised: false,
        // Are similar categories being compared
        comparisonValues: {},
        // Compare similar mode
        compareSimilar: false,
        // Is the comparison option available
        comparisonIsAvailable: false
    }

    // Restore table ordering
    rankingsState.sortAscending = sortAscending;
    rankingsState.sortedColumn = sortedColumn;
}

function displayRankingLegend() {
    var template = MT.model.areaTypeId === PRACTICE
    ? 'bobLegend'
    : 'ragLegend';
    $('#data_legend').html(templates.render(template));
}

function displayPage() {

    initView();

    displayColumnHeadersAndAddDataToRows();

    var rows = rankingsState.rows;
    displayTable(rows);
    displayRankingLegend();
    toggleComparisonHeader();
    setBreadcrumb();
    displayParentAreaName();

    var areaTypeId = MT.model.areaTypeId;
    if (areaTypeId === AreaTypeIds.CCG) {
        $('.external_ccg_link').show();
    }

    $('#ranking-header').html(getRankingHeader());

    // Message in table header
    if (rows.length === 1) {
        var html = '1 ' + getAreaTypeNameSingular(areaTypeId);
    } else {
        html = 'Comparing ' + rows.length + ' ' + getAreaTypeNamePlural(areaTypeId);
    }
    $('#gp_count').html(html);

    $('#area-type-name').html(getAreaTypeNamePlural(areaTypeId));

    $('#similar-areas-tooltip').html(getSimilarAreaTooltipText());

    var comparisonText;
    if (MT.model.parentCode == NATIONAL_CODE) {
        comparisonText = "Compared to other areas";
    } else {
        comparisonText = "Compared to the other practices in the " + getSimpleAreaTypeName();
    }

    $('#comparison-header').html(comparisonText);

    var $hoverOrTap = $('#hover-or-tap');
    if (rankingsState.comparisonIsAvailable) {
        $('#area-type-singular').html(getSimpleAreaTypeNameSingular(areaTypeId));
        $('#area-type-plural').html(getSimpleAreaTypeNamePlural(areaTypeId));
        $hoverOrTap.show();
    } else {
        $hoverOrTap.hide();
    }

    unlock();
}

function displayParentAreaName() {
    var area = loaded.addresses[MT.model.parentCode];

    var areaName = area.AreaTypeId === 15
        ? area.Name
        : 'this ' + getSimpleAreaTypeNameSingular(MT.model.parentAreaType);

    $('.area_name').html(areaName);
}

function displayTable(rows) {

    rows.sort(function (a, b) {
        return a.ValToSortOn - b.ValToSortOn;
    });

    var root = groupRoots[selectedRootIndex];
    sortByPolarity(rows, root.PolarityId);
    sortDisplayedRows(rows);
    
    var column1Val="", 
        column2Val="", 
        column1Unit="", 
        column2Unit="",
        showEnglandVal = false;

    if((MT.model.areaTypeId === AreaTypeIds.CCG) || (MT.model.areaTypeId === AreaTypeIds.CountyUA)) {
        var model = {};
        model.areaCode = MT.model.parentCode;
        model.areaTypeId = MT.model.areaTypeId;
        model.profileId = MT.model.profileId;
        model.groupId = MT.model.groupId;

        var primaryMetadataHash = loaded.indicatorMetadata[model.groupId];
        var supportingMetadataHash = loaded.indicatorMetadata[selectedSupportingGroupId];
        

        // get primary grouping data
        var primaryGroupData = loaded.groupDataAtDataPoint.getData(model);
        column2Val = primaryGroupData[selectedRootIndex].ValF;
        column2Unit = new UnitFormat(primaryMetadataHash[MT.model.indicatorId], column2Val).getLabel();

        // get supporting grouping data
        var selectedSupportingIndicatorId = getSelectedSupportingIndicatorId();
        var supportingIndicatorMetadata = supportingMetadataHash[selectedSupportingIndicatorId];
        model.groupId = selectedSupportingGroupId;
        var supportingGroupData = loaded.groupDataAtDataPoint.getData(model);
        column1Val = supportingIndicatorMetadata.IID === IndicatorIds.Deprivation ?'N/A' : supportingGroupData[selectedSupportingGroupRootIndex].ValF;
        column1Unit = new UnitFormat(supportingIndicatorMetadata, column1Val).getLabel();
        
        showEnglandVal = true;
    }

    // Render data
    $('#diabetes-rankings-table tbody').html(
        templates.render('rows', { rows:rows, englandValCol1:column1Val, englandValCol2:column2Val, col1Unit:column1Unit, col2Unit:column2Unit, showEnglandVal:showEnglandVal })
    );
}

function setBreadcrumb() {
    var links = ['<li><a href="javascript:MT.nav.home();">Home</a></li>'];

    if (MT.model.areaTypeId === PRACTICE) {
        links.push('<li id="national-comparisons"><a href="javascript:switchAreas(' +
            MT.model.parentAreaType + ')">National comparisons</a></li>',
            '<li id="practice-comparisons" class="last"><a>Practice comparisons</a></li>');
    } else {
        links.push('<li id="national-comparisons" class="last"><a>National comparisons</a></li>');
    }

    $('#breadcrumbs').html(links);
}

function getRankingHeader() {
    var areaName = loaded.addresses[MT.model.parentCode].Name;
    return (MT.model.parentCode === NATIONAL_CODE
        ? 'National comparisons'
        : areaName);
}

function getDefaultIndicator() {
    return MT.model.indicatorId = groupRoots[0].IID;
}

function populateAreaTypes(model) {
    var $filter = $('#area-filter');

    if (model.areaTypeId !== PRACTICE) {
        if (_.size(loaded.areaTypes) < 2) {

            $filter.hide();
        } else {
            var templateName = 'areaFilter';

            templates.add(templateName, '<h5><span class="pretext">1.</span><span class="posttext">Select Area</span></h5><ul class="areaFilters">\
            {{#types}}<li id="{{Id}}" class="areaFilter {{class}}"><a href="javascript:switchAreas({{Id}});">{{Short}}</a></li>{{/types}}' + '<div class="hr"><hr /></div>');

            setAreaTypeOptionHtml(templateName);

            $filter.show();
        }
    } else {
        $filter.hide();
    }
}

function switchAreas(parentAreaTypeId) {

    if (!ajaxLock) {
        lock();

        var model = MT.model;

        setUrl('/topic/' + profileUrlKey + '/comparisons#par/' + NATIONAL_CODE + '/ati/' + parentAreaTypeId +
            '/iid/' + model.indicatorId + '/gid/' + model.groupId + '/pat/' + parentAreaTypeId);
        window.location.reload();
    }
}

function createExportLinks() {
    var model = MT.model;
    var serviceUrl = 'GetDataDownload.ashx?pid=' + model.profileId +
        '&ati=' + model.areaTypeId +
        '&tem=' + model.profileId +
        '&pds=0';

    var exportLink = '<a href="' + FT.url.corews + serviceUrl;

    // National data download link
    if (model.areaTypeId !== PRACTICE/*TODO FIN-300 temporarily exclude practices*/) {
        $('#download_data_for_england').html(exportLink + '&pat=15&par=' + NATIONAL_CODE +
            '" class="external_link" target="_blank">Download ' + profileTitle + ' data for England</a>');
    } else {
        //TODO see jira FIN-300, not working for practices, link temporarily hidden
        $('#download_data_for_england').hide();
    }

    // Download practice data within parent area
    if (model.areaTypeId === PRACTICE) {
        $('#download_data_for_ccg').html(exportLink + '&pat=' + model.parentAreaType + '&par=' + model.parentCode +
            '" class="external_link" target="_blank">Download ' + profileTitle + ' data for '
            + loaded.addresses[MT.model.parentCode].Name + '</a>');
    }
}

function getIndexOfGroupRootThatContainsIndicator(indicatorId, groupRoots) {

    var index = 0;

    var selectedIndex = 0;

    _.each(groupRoots, function (root) {
        if (root.IID === indicatorId) {
            selectedIndex = index;
        }
        index++;
    });

    return selectedIndex;
}

function getPracticeList() {
    ajaxGet('data/areas',
        'parent_area_code=' + MT.model.parentCode +
        '&area_type_id=' + MT.model.areaTypeId +
        '&profile_id=' + MT.model.profileId,
        getPracticeListCallback);
};

function getPracticeListCallback(obj) {

    var areasHash = {},
        areaTypeId = MT.model.areaTypeId;

    _.each(obj, function (area) {
        areasHash[area.Code] = area;
    });

    var areaLists = loaded.areaLists;
    if (!areaLists[areaTypeId]) {
        areaLists[areaTypeId] = {};
    }

    areaLists[areaTypeId][MT.model.parentCode] = areasHash;

    ajaxMonitor.callCompleted();
};

function repopulatePrimaryIndicatorList() {
    MT.model.indicatorId = groupRoots[0].IID;
    selectedRootIndex = 0;
    populateCauseList();
    unlock();

    ajaxMonitor.setCalls(1);
    loaded.practiceValues.fetchDataByAjax(groupRoots[ROOT_INDEXES.POPULATION]);
    ajaxMonitor.monitor(selectPrimaryIndicator);
}

function getSupportingIndicators(groupId) {
    var metadataHash = loaded.indicatorMetadata[groupId],
        causes = [], i, metadata;

    var selectedIndicatorId = getSelectedSupportingIndicatorId();

    for (i = 0; i < supportingGroupRoots.length; i++) {

        var indicatorId = supportingGroupRoots[i].IID;
        metadata = metadataHash[indicatorId];

        causes.push({
            index: i,
            name: replacePercentageWithArialFont(metadata.Descriptive.Name),
            id: indicatorId,
            cssClass: indicatorId === selectedIndicatorId
                ? 'sub prev_risk_active'
                : 'sub'
        });
    }

    return causes;
}

function populateSupportingIndicatorList() {
    var html = templates.render('prevandriskcauses',
        { causes: getSupportingIndicators(selectedSupportingGroupId) });
   
    $('#diabetes_prev_and_risk_list-' + selectedSupportingGroupId).html(html);
    setDefaultPrevAndRiskIndicator();
}

function setDefaultPrevAndRiskIndicator() {
    $('#prev-' + getSelectedSupportingIndicatorId()).addClass('prev_risk_active');
    $('#domain-' + selectedSupportingGroupId).addClass('active');
}

function selectDomain(groupId) {

    var model = MT.model;

    if (!ajaxLock && groupId !== model.groupId) {
        lock();

        model.groupId = groupId;

        setSelectedPrimaryDomain(groupId);

        ajaxMonitor.setCalls(3);
        getIndicatorMetadata(groupId);
        getGroupRoots(model);
        getEnglandPrimaryData(model);
        ajaxMonitor.monitor(repopulatePrimaryIndicatorList);
    }
}

function selectSupportingDomain(groupId) {

    if (!ajaxLock && selectedSupportingGroupId !== groupId) {
        lock();

        selectedSupportingGroupId = groupId;

        // Set first indicator to be selected
        selectedSupportingGroupRootIndex = 0;

        var model = MT.model;

        ajaxMonitor.setCalls(3);

        getSupportingGroupRoots(model);
        getIndicatorMetadata(selectedSupportingGroupId);

        ajaxMonitor.monitor(repopulateSupportingIndicatorList);

        displaySupportingIndicatorSelection();
        getEnglandSupportingData(model);
        unlock();
    }
}

function displaySupportingIndicatorSelection() {
    // Display new active options
    var cssActiveClass = 'active';

    var groupId = selectedSupportingGroupId;
    $('.prev_and_risk_filters').hide();
    $('#diabetes_prev_and_risk_list-' + groupId).show();
    $('.prev_and_risk_indicator_group').removeClass(cssActiveClass);
    $('#domain-' + groupId).addClass(cssActiveClass);
}

function repopulateSupportingIndicatorList() {
    populateSupportingIndicatorList();
    selectSupportingIndicator(0);
}

function getSupportingGroupRoots(model) {
    //Reload group roots each time to ensure clicking on a grouping reloads the relevant indicators
    getData(function (obj) {
        supportingGroupRoots = obj;
        ajaxMonitor.callCompleted();
    }, 'gr',
        'gid=' + selectedSupportingGroupId +
        '&ati=' + model.areaTypeId);
}

function getEnglandPrimaryData(model) {
    var modelLocal = {}
    modelLocal.areaCode = model.parentCode;
    modelLocal.areaTypeId = model.areaTypeId;
    modelLocal.profileId = model.profileId;
    modelLocal.groupId = model.groupId;
    loaded.groupDataAtDataPoint.fetchDataByAjax(modelLocal);
}

function getEnglandSupportingData(model) {
    var modelLocal = {}
    modelLocal.areaCode = model.parentCode;
    modelLocal.areaTypeId = model.areaTypeId;
    modelLocal.profileId = model.profileId;
    modelLocal.groupId = selectedSupportingGroupId;
    loaded.groupDataAtDataPoint.fetchDataByAjax(modelLocal);
}


function selectPrimaryIndicator(rootIndex) {

    if (!ajaxLock) {
        lock();

        var model = MT.model;

        rootIndex = isDefined(rootIndex)
    		? rootIndex
            : 0;
        selectedRootIndex = rootIndex;

        model.indicatorId = groupRoots[rootIndex].IID;
        isSupportingIndicatorSelected = false;

        ajaxMonitor.setCalls(1);
        loaded.primaryDataValues.fetchDataByAjax(groupRoots[selectedRootIndex]);
        ajaxMonitor.monitor(reloadData);

        setSelectedPrimaryIndicator();
    }
}

function setSelectedPrimaryIndicator() {
    selectedCause = $('#iid-' + MT.model.indicatorId);
    var cssClass = 'active';
    $('.causes li').removeClass(cssClass);
    selectedCause.addClass(cssClass);
}

function selectSupportingIndicator(rootIndex) {
    if (!ajaxLock) {
        lock();

        var indicatorId = supportingGroupRoots[rootIndex].IID;
        selectedSupportingGroupRootIndex = rootIndex;
        isSupportingIndicatorSelected = true;

        ajaxMonitor.setCalls(1);
        getSupportingDataValues(selectedSupportingGroupRootIndex);
        ajaxMonitor.monitor(reloadData);

        setSelectedSupportingIndicator(indicatorId);
    }
}

function setSelectedSupportingIndicator(indicatorId) {
    selectedCause = $('#prev-' + indicatorId);
    var cssClass = 'prev_risk_active';
    $('.' + cssClass).removeClass(cssClass);
    selectedCause.addClass(cssClass);
}

function selectPractice(code) {
    rankingsState.compareSimilar = false;

    var model = MT.model;
    if (hasPracticeData) {
        if (!ajaxLock) {
            lock();
            model.parentCode = model.parentCode;
            model.areaCode = code;
            model.areaTypeId == PRACTICE ? MT.nav.practiceDetails(model) : MT.nav.rankings();
            scrollToTop();
        }
    } else {
        model.areaCode = code;
        MT.nav.areaDetails(model);
    }
}

function showAllAreas() {

    if (!ajaxLock) {
        lock();

        rankingsState.compareSimilar = false;
        toggleComparisonHeader();
        reloadData();

        logEvent(AnalyticsCategories.RANKINGS, AnalyticsAction.ALL_AREAS);
    }
}

function getNewRowsWithCoreData() {

    var areaCodeToRowHash = createRows();

    // Add data to rows
    var root = groupRoots[selectedRootIndex];
    var primaryDataList = loaded.primaryDataValues.getData(root);
    var supportingDataList = loaded.supportingDataValues[MT.model.areaTypeId];

    var isPrimaryData = primaryDataList.length > 0;
    var isSupportingData = supportingDataList.length > 0;

    var dataList = isPrimaryData
        ? primaryDataList
        : supportingDataList;

    for (var i in dataList) {

        row = areaCodeToRowHash[dataList[i].AreaCode];

        if (row) {
            row.PrimaryData = isPrimaryData && new CoreDataSetInfo(primaryDataList[i]).isValue()
             ? primaryDataList[i]
             : null;

            row.SupportingData = isSupportingData && new CoreDataSetInfo(supportingDataList[i]).isValue()
                ? supportingDataList[i]
                : null;
        }
    }
}

function initView() {
    var state = rankingsState;

    if (!state.isInitialised) {
        state.isInitialised = 1;
        displayInfoBoxes();
    }
}

function displayInfoBoxes() {

    var supportingAreaData = loaded.supportingAreaData.getData(
    {
        profileId: SupportingProfileId,
        groupId: SupportingGroupId,
        areaCode: MT.model.parentCode
    });
    var ranks = supportingAreaData.Ranks[NATIONAL_CODE];

    switch (MT.model.profileId) {
        case ProfileIds.HealthChecks:
            displayInfoBox1(ranks[1]);
            break;
        case ProfileIds.DrugsAndAlcohol:
            displayInfoBox2(ranks[2]);
            break;
        default:
            displayInfoBox1(ranks[1]);
            displayInfoBox2(ranks[0]);
    }
}

function displayInfoBox1(rank) {

    var html = templates.renderOnce(
            '<h2>Population of <span class="area_name"></span></h2><p><span>{{count}}</span></p>',
            {
                count: new CommaNumber(rank.AreaRank.Count).rounded()
            });

    $('#info_box_1').html(html);
}

function displayInfoBox2(rank) {

    var templateModel = {};

    if (MT.model.profileId === ProfileIds.DrugsAndAlcohol) {
        var count = new CommaNumber(rank.AreaRank.Count).rounded();
        var template = '<h2>Estimated number of opiate and/or crack cocaine users in England</h2><p><span>{{count}}</span> {{period}}</p>';
        
    } else {

        count = isDefined(rank.AreaRank)
            ? new CommaNumber(rank.AreaRank.Count).rounded()
            : 'Data unavailable';

        template = '<h2>Adults in <span class="area_name"></span> with {{condition}}</h2><p><span>{{count}}</span> {{period}}</p>';
        templateModel.condition = getConditionWord();
    }

    templateModel.period = rank.Period;
    templateModel.count = count;

    $('#info_box_2').html(templates.renderOnce(template, templateModel)).show();
}

function setPrimaryDataHeader(metadata) {

    if (metadata.IID == IndicatorIds.Deprivation) {
        var columnHeader = getDeprivationColumnHeader();
    } else {
        columnHeader = new ColumnHeader(metadata).text
    }

    $('#value_type_heading').html(columnHeader +
            '<i style="right: -1.3em;"></i>'/*sorted by triangle*/);
}

function assignDataLabelsToRows(primaryMetadata, supportingMetadata) {

    getNewRowsWithCoreData();
    var rows = rankingsState.rows;

    var isSupportingDataUnit = isSupportingDataUnitRequired();
    var isDeprivation = isDeprivationSelected();

    var sortedColumn = rankingsState.sortedColumn;
    var sortOnPrimaryData = sortedColumn === 2;
    var sortOnSecondaryData = sortedColumn === 1;

    var root = groupRoots[selectedRootIndex];
    var gradeFunction = getGradeFunctionFromGroupRoot(root);

    for (var i in rows) {
        var row = rows[i];
        var valToSortOn = -1;

        // Primary data
        if (row.PrimaryData) {
            var data = row.PrimaryData;
            row.primaryDataText = getDataText(data, root.IID === IndicatorIds.Deprivation);
            row.unitLabel = new UnitFormat(primaryMetadata, data.Val).getLabel();
            if (sortOnPrimaryData) {
                valToSortOn = data.Val;
            }

            row.grade = gradeFunction(data.Sig[-1], root);

            if (!new CoreDataSetInfo(data).isNote()) {
                row.primaryValueNote = '';
            } else {
                row.primaryValueNote = '<span class="tooltip primary-value-note-tooltip"><i>' + getValueNoteText(data.NoteId) + '</i></span>';
                if (data.NoteId === 401) {
                    row.grade = 'grade-99';
                }
            }
        } else {
            row.primaryValueNote = '';
            row.primaryDataText = 'No data';
            row.unitLabel = '';
            row.grade = gradeFunction(0, root);
        }

        // Supporting data
        if (row.SupportingData) {
            var supportingData = row.SupportingData;
            row.supportingDataText = getDataText(supportingData, isDeprivation);
            row.supportingGrade = supportingData.Sig[-1];
            row.supportingDataUnitLabel = isSupportingDataUnit
            ? new UnitFormat(supportingMetadata, supportingData.Val).getLabel()
                : '';
            if (sortOnSecondaryData) {
                valToSortOn = supportingData.Val;
            }

            if (!new CoreDataSetInfo(supportingData).isNote()) {
                row.supportingValueNote = '';
            } else {
                row.supportingValueNote = '<span id="supporting-value-note-tooltip" class="tooltip"><i>' + getValueNoteText(supportingData.NoteId) + '</i></span>';
            }
        } else {
            row.supportingValueNote = '';
            row.supportingDataText = 'No data';
            row.supportingGrade = 0;
            row.supportingDataUnitLabel = '';
        }
        row.supportingSignificanceImage = getSupportingGrade(row.supportingGrade);

        row.ValToSortOn = valToSortOn;
    }
}

function getDataText(data, isDeprivation) {
    if (isDeprivation) {
        return deprivationDefinitions[data.Sig[-1]];
    }

    return data.ValF;
}

function getSupportingGrade(grade) {

    // Work out if this is showing Quintile (Purple) or RAG for the middle column
    var supportingGroupRoot = supportingGroupRoots[selectedSupportingGroupRootIndex];
    var comparatorMethodId = supportingGroupRoot.Grouping[0].MethodId;

    if (useQuintiles(comparatorMethodId)) {
        var supportingSignificanceImage = '<img src="' + FT.url.img +
            'Mortality/grade-quintile-' + grade + '.png" />';
    } else {
        //RAG middle column
        var gradeFunction = getGradeFunctionFromGroupRoot(supportingGroupRoot);
        var gradeName = gradeFunction(grade, supportingGroupRoot);
        supportingSignificanceImage = '<img src="' + FT.url.img +
            'Mortality/' + gradeName + '.png" />';
    }

    return supportingSignificanceImage;
}

function setComparisonLink(areaCode) {
    if (rankingsState.comparisonIsAvailable) {
        return '<div class="compare-link"><a href="javascript:selectSimilarAreas(\''
            + areaCode + '\');">Compare similar</a></div>';
    }

    return '';
}

function setSupportingDataHeader(metadata) {
    if (metadata.IID == IndicatorIds.Deprivation) {
        var columnHeader = getDeprivationColumnHeader();
    } else {
        columnHeader = new ColumnHeader(metadata).text;
    }

    // Set column heading
    $('#supporting_data_heading').html('<a href="javascript:sortRankings(1);">' +
        columnHeader + '<i></i></a>');
}

function isDeprivationSelected() {
    return getSelectedSupportingIndicatorId() === IndicatorIds.Deprivation;
}

function isSupportingDataUnitRequired() {
    return !isDeprivationSelected(); // no unit for deprivation labels
}

function reloadData() {

    displayColumnHeadersAndAddDataToRows();

    var rows = rankingsState.rows;
    if (rankingsState.compareSimilar) {
        rows = setComparisonValues(rows);
    }

    displayTable(rows);
    displayParentAreaName();

    unlock();
}

function setComparisonValues(rows) {
    var practiceName = loaded.areaLists[MT.model.areaTypeId][MT.model.parentCode][rankingsState.comparisonValues.selectedAreaCode].Name;

    $('#comparing_practice_name').html(practiceName);
    toggleComparisonHeader();

    var similarRows = [];
    var idx = 0;
    _.each(rows, function (row) {

        switch (MT.model.areaTypeId) {
            case AreaTypeIds.CountyUA:
                var rowDecile = getDecileInfo(loaded.categories[AreaTypeIds.DeprivationDecile][row.Code]);
                if (rankingsState.comparisonValues[rankingsState.comparisonValues.selectedAreaCode] == rowDecile.decile) {
                    similarRows.push(row);
                }
                break;
            case AreaTypeIds.CCG:
                var areaCategory = loaded.categories[AreaTypeIds.DeprivationDecile][row.Code];
                if (rankingsState.comparisonValues[rankingsState.comparisonValues.selectedAreaCode] == areaCategory) {
                    similarRows.push(row);
                }
                break;
            case PRACTICE:
                if (isDefined(loaded.practiceCategories[row.Code])) {
                    if (loaded.practiceCategories[row.Code].Code === rankingsState.comparisonValues.Code) {
                        similarRows.push(row);
                    }
                }
                break;
        }
        idx++;
    });

    return similarRows;
}

function toggleComparisonHeader() {
    if (rankingsState.compareSimilar) {
        $('.non-similar').hide();
        $('.similar').show();
    } else {
        $('.non-similar').show();
        $('.similar').hide();
    }
}

function sortRankings(columnIndex) {

    changeSortState(columnIndex);
    displaySortArrowOnTableHeader(columnIndex);

    if (rankingsState.compareSimilar) {
        reloadData();
        hideCompareSimilarLink();
    } else {
        displayPage();
    }
}

function changeSortState(columnIndex) {

    var state = rankingsState;
    var currentColumn = state.sortedColumn;

    if (currentColumn !== columnIndex) {
        // Change sorted column   
        state.sortedColumn = columnIndex;
        state.sortAscending = true;
    } else {
        // Change sort order
        state.sortAscending = !state.sortAscending;
    }
}

function displaySortArrowOnTableHeader(columnIndex) {
    var $headers = $('#diabetes-rankings-table TH');

    // Remove all sorted classes
    $headers.removeClass('sorted sorted-desc');

    // Apply style to selected header
    var $column = $($headers[columnIndex]);
    $column.addClass('sorted');
    if (!rankingsState.sortAscending) {
        $column.addClass('sorted-desc');
    }

}

function sortDisplayedRows(rows) {

    var sortAscending = rankingsState.sortAscending;
    var sortedColumn = rankingsState.sortedColumn;

    if (sortedColumn === 0) {
        // Sort on area name
        rows.sort(sortAreasByName);
    } else if (sortedColumn === 2) {
        // Sort on supporting data
        rows.sort(function (a, b) {
            return a.ValToSortOn - b.ValToSortOn;
        });
    } else if (sortedColumn === 1) {
        // Sort on Totals if last column is selected
        rows.sort(function (a, b) {
            return a.ValToSortOn - b.ValToSortOn;
        });
    }

    if (!sortAscending) {
        rows.reverse();
    }
}

function sortCategory(a, b) {
    return sortString(a.supportingDataText, b.supportingDataText);
};

function sortByPolarity(rowsToDisplay, polarity) {
    if (polarity !== 0) {
        rowsToDisplay.reverse();
    }
}

function selectSimilarAreas(areaCode) {
   
    if (!ajaxLock) {
        lock();

        switch (MT.model.areaTypeId) {
            case AreaTypeIds.CountyUA:
                var depDecile = loaded.categories[AreaTypeIds.DeprivationDecile][areaCode];
                $('#comparison_category').html('(in Socioeconomic decile ' +
                        depDecile + ' - ' + getDecileInfo(depDecile).text + ')');
                $('#comparing_area_type').html('to similar areas');
                $('#comparison_type').html('Similar Areas');

                rankingsState.comparisonValues = loaded.categories[AreaTypeIds.DeprivationDecile];
                rankingsState.comparisonValues.selectedAreaCode = areaCode;
                rankingsState.compareSimilar = true;
                break;
            case PRACTICE:
                $('#comparison_category').html('(' + loaded.practiceCategories[areaCode].Name.trim() + ')');
                $('#comparing_area_type').html('to similar practices');
                $('#comparison_type').html('Similar Practices');

                rankingsState.comparisonValues = loaded.practiceCategories[areaCode];
                rankingsState.comparisonValues.selectedAreaCode = areaCode;
                rankingsState.compareSimilar = true;
                break;
            case AreaTypeIds.CCG:
                $('#comparing_area_type').html('to similar areas');
                $('#comparison_type').html('Similar Areas');

                rankingsState.comparisonValues = loaded.categories[AreaTypeIds.DeprivationDecile];
                rankingsState.comparisonValues.selectedAreaCode = areaCode;
                rankingsState.compareSimilar = true;
                break;
        }
        scrollToTop();
        reloadData();
        hideCompareSimilarLink();

        logEvent(AnalyticsCategories.RANKINGS, AnalyticsAction.SIMILAR_AREAS);
    }
}

function hideCompareSimilarLink() {
    $('.compare-link').removeClass('show-compare').addClass('hide-compare');
}

function scrollToTop() {
    // Ensure top of table can be seen
    var $window = $(window);
    if ($window.scrollTop() > 600) {
        $window.scrollTop(260);
    }
}

function changeNumberOrder() {

    var titleOrder = 1;
    var riskOrder = 2;

    if (MT.model.areaTypeId !== PRACTICE && _.size(loaded.areaTypes) > 1) {
        titleOrder++;
        riskOrder++;
    }

    $('#indicator-title-order').text(titleOrder + '.');
    $('#prevalence-risk-order').text(riskOrder + '.');
}

function goToPracticeProfiles() {
    var url = 'http://fingertips.phe.org.uk/profile/general-practice/data#mod,2,pyr,2013,pat,19,par,' +
        MT.model.parentCode + ',are,-,sid1,2000005,ind1,-,sid2,-,ind2,-';
    window.open(url);
}

function setSelectedPrimaryDomain(groupId) {
    // Display new active options
    var cssActiveClass = 'active';

    // Hide current indicator list
    var indicatorGroups = $('.filters .causes');
    indicatorGroups.hide();

    // Remove selected from current indicator
    $('.filters li').removeClass(cssActiveClass);

    // Select new domain
    $('#domain-' + groupId).addClass(cssActiveClass);

    // Show list of indicators in selected domain
    var $currentList = $('#diabetes_list-' + groupId);
    $currentList.show();
    $currentList.addClass(cssActiveClass);
}

function getSelectedSupportingIndicatorId() {
    return supportingGroupRoots[selectedSupportingGroupRootIndex].IID;
}

function setGlobalIndicatorIds() {

    var model = MT.model;

    if (!isDefined(model.indicatorId)) {
        // No indicator specified in hash
        model.indicatorId = getDefaultIndicator();
        selectedRootIndex = 0;
        selectedSupportingGroupRootIndex = 0;
    } else if (isSupportingIndicatorSelected) {
        // Indicator in hash is in a supporting domain
        selectedSupportingGroupRootIndex = getIndexOfGroupRootThatContainsIndicator(
            model.indicatorId, supportingGroupRoots);
        model.indicatorId = getDefaultIndicator();
        selectedRootIndex = 0;
    } else {
        // model.indicatorId already is set to an indicator in a primary domain
        selectedSupportingGroupRootIndex = 0;
        selectedRootIndex = getIndexOfGroupRootThatContainsIndicator(
            model.indicatorId, groupRoots);
    }
}

function createRows() {

    // Define rows
    var rows = rankingsState.rows = [];
    var areaCodeToRowHash = {};
    var areas = loaded.areaLists[MT.model.areaTypeId][MT.model.parentCode];
    for (var i in areas) {

        var areaCode = areas[i].Code;

        var row = {
            Name: areas[areaCode].Name,
            Code: areas[areaCode].Code,
            compareSimilar: setComparisonLink(areaCode)
        };

        areaCodeToRowHash[areaCode] = row;
        rows.push(row);
    }

    return areaCodeToRowHash;
}

function displayColumnHeadersAndAddDataToRows() {

    var model = MT.model;

    var primaryMetadataHash = loaded.indicatorMetadata[model.groupId];
    var supportingMetadataHash = loaded.indicatorMetadata[selectedSupportingGroupId];

    var primaryMetadata = primaryMetadataHash[model.indicatorId];
    var supportingMetadata = supportingMetadataHash[getSelectedSupportingIndicatorId()];

    setPrimaryDataHeader(primaryMetadata);
    setSupportingDataHeader(supportingMetadata);

    assignDataLabelsToRows(primaryMetadata, supportingMetadata);
}

function ColumnHeader(metadata) {

    var unit = metadata.Unit;
    var unitLabel = unit.Id === 5
        ? '' :
        unit.Label;

    this.text = replacePercentageWithArialFont(metadata.Descriptive.Name) +
        '<small>' + unitLabel + '</small>';
}

function getDeprivationColumnHeader() {

    var model = MT.model;
    var parentAreaType = model.areaTypeId === PRACTICE
    ? model.parentAreaType
        : AreaTypeIds.Country;

    return getDeprivationLabel('Deprivation', parentAreaType);
}

templates.add('causes',
    '{{#causes}}<li id=iid-{{id}} class="{{cssClass}}"><a href="javascript:selectPrimaryIndicator({{index}})">{{{name}}}</a></li>{{/causes}}');

templates.add('prevandriskcauses',
    '{{#causes}}<li id=prev-{{id}} class="{{cssClass}}"><a href="javascript:selectSupportingIndicator({{index}})">{{{name}}}</a></li>{{/causes}}');

templates.add('rows',
    '{{#showEnglandVal}}<tr class="england-row"><td><span class="england-label">England</span></td><td><span class="england-val">{{englandValCol1}}{{{col1Unit}}}</span></td><td><span class="england-val">{{englandValCol2}}{{{col2Unit}}}</span></td></td>{{/showEnglandVal}}\
    {{#rows}}<tr class="odd {{selected}}"><td>\
    <a href="javascript:selectPractice(\'{{Code}}\')">{{Name}}</a></td>\
    <td><span class="grade grade-quintile-{{supportingGrade}}">{{{supportingSignificanceImage}}}</span>{{supportingDataText}}<span>{{{supportingDataUnitLabel}}}</span>{{{supportingValueNote}}}</td>\
    <td class="last-child"><span class="grade {{grade}}"><img src="' + FT.url.img + 'Mortality/{{grade}}.png" /></span><span>{{primaryDataText}}</span><span>{{{unitLabel}}}</span>{{{primaryValueNote}}}{{{compareSimilar}}}</td></tr>{{/rows}}');

templates.add('bobLegend',
'<p class="legend">Comparison with area average\
    <span class="grade">\
        <img src="' + FT.url.img + 'Mortality/bobLower.png" alt="worse" />lower\
    </span>\
    <span class="grade">\
        <img src="' + FT.url.img + 'Mortality/grade-2.png" alt="consistent" />consistent\
    </span>\
    <span class="grade">\
        <img src="' + FT.url.img + 'Mortality/bobHigher.png" alt="better" />higher\
    </span>\
</p>');

templates.add('ragLegend',
'<p class="legend">Comparison with national average\
    <span class="grade">\
        <img src="' + FT.url.img + 'Mortality/grade-3.png" alt="worse" />worse\
    </span>\
    <span class="grade">\
        <img src="' + FT.url.img + 'Mortality/grade-2.png" alt="consistent" />consistent\
    </span>\
    <span class="grade">\
        <img src="' + FT.url.img + 'Mortality/grade-0.png" alt="better" />better\
    </span>\
</p>' +
'<p class="legend">Comparison with national average\
    <span class="grade">\
        <img src="' + FT.url.img + 'Mortality/bobLower.png" alt="worse" />lower&nbsp;\
    </span>\
    <span class="grade">\
        <img src="' + FT.url.img + 'Mortality/grade-2.png" alt="consistent" />consistent\
    </span>\
    <span class="grade">\
        <img src="' + FT.url.img + 'Mortality/bobHigher.png" alt="better" />higher\
    </span>\
</p>'

);

// Global state of supporting indicator selection
isSupportingIndicatorSelected = false;
selectedSupportingGroupRootIndex = -1;
selectedSupportingGroupId = -1;

loaded.supportingDataValues = {};
loaded.primaryDataValues = new AreaValuesDataManager();
loaded.practiceValues = new AreaValuesDataManager();
loaded.specificDomainAreaDetails = new AreaDetailsDataManager();
loaded.groupDataAtDataPoint = new GroupDataAtDataPointOfSpecificAreasDataManager();
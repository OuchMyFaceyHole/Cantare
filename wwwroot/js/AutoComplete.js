var workingSet = [];
var currentActiveElement = null;
var currentActiveIndex = -1;
var currentlySelectedInput = null;
var openAutoCompleteList = [];
var openAutoCompleteListContainer = null;

$(document).ready(function () {
    currentlySelectedInput = document.getElementById("SongInput");
    document.addEventListener('input', function (event) {
        if (event.target.classList.contains("AutoCompleteInput")) {
            let url = '/GetSongNames';

            if (event.target.value.length > 0) {
                $.ajax({
                    url: url,
                    data: { songTitleStart: event.target.value },
                    async: true,
                    success: function (data) {
                        workingSet = [];
                        $.each(JSON.parse(data), function (index, song) {
                            workingSet.push(song);
                        });
                        AutoComplete(event.target);
                    }
                });
            }
            else {
                CloseAllLists();
                workingSet = [];
            }
            return;
        }

    });
    document.addEventListener('click', function (event) {
        if (event.target.classList.contains("AutoCompleteListItem")) {
            currentlySelectedInput.value = event.target.querySelectorAll('input')[0].dataset.visualValue;
            currentlySelectedInput.dataset.trueValue = event.target.querySelectorAll('input')[0].value;
            CloseAllLists();
        }
    });
    document.addEventListener('keydown', function (event) {
        if (currentlySelectedInput) {
            if (event.keyCode == 40) {
                /*If the arrow DOWN key is pressed,
                increase the currentFocus variable:*/
                currentActiveIndex++;
                /*and and make the current item more visible:*/
                AddActive();
            }
            else if (event.keyCode == 38) { //up
                /*If the arrow UP key is pressed,
                decrease the currentFocus variable:*/
                currentActiveIndex--;
                /*and and make the current item more visible:*/
                AddActive();
            }
            else if (event.keyCode == 13) {
                /*If the ENTER key is pressed, prevent the form from being submitted,*/
                e.preventDefault();
                if (currentActiveIndex > -1) {
                    /*and simulate a click on the "active" item:*/
                    if (openAutoCompleteList) {
                        openAutoCompleteList[currentActiveIndex].click();
                    }
                }
            }
        }
    });
});

function AutoComplete(inputElement) {
    /*close any already open lists of autocompleted values*/
    CloseAllLists();
    if (inputElement.value.length < 1) {
        return false;
    }
    currentActiveIndex = -1;
    /*create a DIV element that will contain the items (values):*/
    var newAutoCompleteContainer = document.createElement("DIV");
    newAutoCompleteContainer.setAttribute("class", "autocomplete-items");
    openAutoCompleteListContainer = newAutoCompleteContainer;
    /*append the DIV element as a child of the autocomplete container:*/
    inputElement.parentNode.appendChild(newAutoCompleteContainer);
    var containerCount = 0;
    /*for each item in the array...*/
    $.each(workingSet, function (index, name) {
        /*create a DIV element for each matching element:*/
        var listItemContainer = document.createElement("DIV");
        listItemContainer.classList.add("AutoCompleteListItem");

        var nameSplit = name.split('/');

        var nameBase = document.createElement("span");
        nameBase.innerHTML = nameSplit[0] + ' ' + nameSplit[1] + ' ';
        nameBase.style.pointerEvents = "none";
        listItemContainer.appendChild(nameBase);

        var nameStrong = document.createElement("strong");
        nameStrong.innerHTML = nameSplit[2].substr(0, inputElement.value.length);
        nameStrong.style.pointerEvents = "none";
        listItemContainer.appendChild(nameStrong);

        var songTitleWeak = document.createElement("span");
        songTitleWeak.innerHTML = nameSplit[2].substr(inputElement.value.length, nameSplit[2].length);
        songTitleWeak.style.pointerEvents = "none";
        listItemContainer.appendChild(songTitleWeak);

        var hiddenInput = document.createElement("input");
        hiddenInput.hidden = true;
        hiddenInput.value = name;
        hiddenInput.dataset.visualValue = nameSplit[0] + ' ' + nameSplit[1] + ' ' + nameSplit[2];
        listItemContainer.appendChild(hiddenInput);
       
        /*execute a function when someone clicks on the item value (DIV element):*/
        openAutoCompleteListContainer.appendChild(listItemContainer);
        openAutoCompleteList.push(listItemContainer);
        currentActiveIndex = index;
        containerCount++;
    });
}
function AddActive() {
    /*a function to classify an item as "active":*/
    if (!x) return false;
    /*start by removing the "active" class on all items:*/
    RemoveActive();
    if (currentActiveIndex >= x.length) {
        currentActiveIndex = 0;
    }
    if (currentActiveIndex < 0) {
        currentActiveIndex = (x.length - 1);
    }
    /*add class "autocomplete-active":*/
    openAutoCompleteList[currentActiveIndex].classList.add("autocomplete-active");
    currentActiveElement = openAutoCompleteList[currentActiveIndex];
}
function RemoveActive() {
    currentActiveElement.classList.remove("autocomplete-active");
}
function CloseAllLists() {
    if (openAutoCompleteListContainer != null) {
        openAutoCompleteListContainer.remove();
        openAutoCompleteList = [];
        currentActiveElement = null;
        currentActiveIndex = -1;
    }
}
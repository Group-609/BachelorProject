mergeInto(LibraryManager.library, {
	Save: function (message) {
		insertData(Pointer_stringify(message));
	},

	FirstConditionFinished: function (gatheredData) {
		showSecondForm(Pointer_stringify(gatheredData));
	}, 

	SecondConditionFinished: function (gatheredData) {
		showLastForm(Pointer_stringify(gatheredData));
	}, 
});
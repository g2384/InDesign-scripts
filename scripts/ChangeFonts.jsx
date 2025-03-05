// Define fonts
var latinFont = "Calibri"; // Change to your preferred Latin font
var chineseFont = "KaiTi"; // Change to your preferred Chinese font
var numberFont = "Calibri"; // Change to your preferred Number font
var fontSize = 20; // pt
var leading = 18; // pt
var justification = Justification.LEFT_ALIGN; // Align left

(function () {
    var doc = app.activeDocument;
    var sel = app.selection;

    if (!sel || sel.length === 0 || !(sel[0] instanceof TextFrame)) {
        alert("Please select a text frame.");
        return;
    }

    var textFrame = sel[0];
    var text = textFrame.texts[0];

    // Regular expressions for character matching
    var latinRegex = /[A-Za-z]/;
    var chineseRegex = /[\u4E00-\u9FFF]/; // Unicode range for Chinese characters
    var numberRegex = /[0-9]/;
    var punctuationRegex = /[\.,!?;:'"()]/; // Punctuation characters

    // Apply paragraph formatting
    text.pointSize = fontSize; // Font size
    text.leading = leading; // Leading
    text.justification = justification; // Align left

    var errorSamples = {}; // Store up to 3 unique characters per font issue

    function contains(arr, char2) {
        // Check if the character already exists in the array
        for (var j = 0; j < arr.length; j++) {
            if (arr[j] === char2) {
                return true;
            }
        }
        return false;
    }

    function logError(fontName, char2) {
        if (!errorSamples[fontName]) {
            errorSamples[fontName] = [];
        }
        if (errorSamples[fontName].length < 3 && !contains(errorSamples[fontName], char2)) {
            errorSamples[fontName].push(char2);
        }
    }

    // Iterate through characters
    for (var i = 0; i < text.characters.length; i++) {
        var char2 = text.characters[i].contents;
        try {
            if (latinRegex.test(char2)) {
                text.characters[i].appliedFont = latinFont;
            } else if (chineseRegex.test(char2)) {
                text.characters[i].appliedFont = chineseFont;
            } else if (numberRegex.test(char2)) {
                text.characters[i].appliedFont = numberFont;
            } else if (punctuationRegex.test(char2)) {
                text.characters[i].appliedFont = latinFont; // Use Latin font for punctuation
            }
        } catch (e) {
            logError(e.message, char2); // Log only unique 3 characters per missing font
        }
    }

    // Count error entries
    var errorCount = 0;
    for (var key in errorSamples) {
        if (errorSamples.hasOwnProperty(key)) {
            errorCount++;
        }
    }

    // Display errors if any
    if (errorCount > 0) {
        var errorMessage = "Font errors occurred:\n";
        for (var fontError in errorSamples) {
            if (errorSamples.hasOwnProperty(fontError)) {
                errorMessage += fontError + " (examples: " + errorSamples[fontError].join(", ") + ")\n";
            }
        }
        alert(errorMessage);
    }
})();

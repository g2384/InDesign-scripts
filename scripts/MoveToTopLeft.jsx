//An InDesign JavaScript
/*  
@@@BUILDINFO@@@ "moveToTopLeft.jsx" 1.0.0 30 March 2022
*/

main();
function main() {
    //Make certain that user interaction (display of dialogs, etc.) is turned on.
    app.scriptPreferences.userInteractionLevel = UserInteractionLevels.interactWithAll;
    var objects = new Array;
    if (app.documents.length != 0) {
        if (app.selection.length != 0) {
            for (var i = 0; i < app.selection.length; i++) {
                switch (app.selection[i].constructor.name) {
                    case "Rectangle":
                    case "Oval":
                    case "Polygon":
                    case "GraphicLine":
                    case "TextFrame":
                        objects.push(app.selection[i]);
                        break;
                }
            }
            if (objects.length != 0) {
                moveToTopLeft(objects);
            } else {
                alert("Please select a rectangle, oval, polygon, text frame, or graphic line and try again.");
            }
        } else {
            alert("Please select an object and try again.");
        }
    } else {
        alert("Please open a document, select an object, and try again.");
    }
}

function moveToTopLeft(objs) {
    for (var i = 0; i < objs.length; i++) {
        var o = objs[i];
        var page = o.parentPage;
        var margins = page.marginPreferences;
        var leftMargin = margins.left;
        
        // Get the first horizontal grid line
        var firstGridLine = getFirstHorizontalGuide(page);
        if (firstGridLine !== null) {
            var topMargin = firstGridLine;
        } else {
            var topMargin = margins.top; // Fallback to default top margin if no grid found
        }

        var currY = o.geometricBounds[0]; //top
        var currY2 = o.geometricBounds[2]; //bottom
        var currWidth = o.geometricBounds[3] - o.geometricBounds[1];
        var currHeight = currY2 - currY;
        o.geometricBounds = [topMargin, leftMargin, currHeight + topMargin, currWidth + leftMargin];
    }
}

// Function to get the first horizontal guide line from the page
function getFirstHorizontalGuide(page) {
    var guides = page.guides;
    
    // Find the first horizontal guide
    for (var i = 0; i < guides.length; i++) {
        if (guides[i].orientation === HorizontalOrVertical.HORIZONTAL) {
            return guides[i].location; // Return the position of the first horizontal guide
        }
    }

    return null; // Return null if no horizontal guides are found
}

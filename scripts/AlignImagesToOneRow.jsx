//AlignImagesToOneRow.jsx
//An InDesign JavaScript
/*  
@@@BUILDINFO@@@ "AlignImagesToOneRow.jsx" 1.0.0 30 March 2022
*/

main();

function main() {
  //Make certain that user interaction (display of dialogs, etc.) is turned on.
  app.scriptPreferences.userInteractionLevel =
    UserInteractionLevels.interactWithAll;
  var objects = new Array();
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
        alignToOneRow(objects);
      } else {
        alert(
          "Please select a rectangle, oval, polygon, text frame, or graphic line and try again."
        );
      }
    } else {
      alert("Please select an object and try again.");
    }
  } else {
    alert("Please open a document, select an object, and try again.");
  }
}

function getGuide(page, orientation) {
  var guides = [];
  for (var i = 0; i < page.masterPageItems.length; i++) {
    var item = page.masterPageItems[i];
    if (item.hasOwnProperty("location") && item.orientation == orientation) {
      guides.push(item.location);
    }
  }
  guides = guides.sort(function (a, b) {
    return a - b;
  });
  return guides;
}

function sum(arr) {
  var total = 0;
  for (var i = 0; i < arr.length; i++) {
    total += arr[i];
  }
  return total;
}

function alignToOneRow(objs) {
  var n = objs.length;
  var firstO = objs[0];
  var pg = firstO.parentPage;
  var guides = getGuide(pg, HorizontalOrVertical.HORIZONTAL);
  var startPosY = guides[3];
  var leftMargin = pg.marginPreferences.left;
  var rightMargin = pg.marginPreferences.right;
  var pageW = pg.bounds[3] - pg.bounds[1];
  var startPos = leftMargin;
  var endPos = pageW - rightMargin;

  var imageTotalWidth = endPos - startPos;

  var margin = 2.5;

  var imagesWidth = [];
  for (var i = 0; i < objs.length; i++) {
    var bounds = objs[i].geometricBounds;
    imagesWidth.push(bounds[3] - bounds[1]);
  }

  var totalImagesWidth = sum(imagesWidth);
  var actualTotalImagesWidth = imageTotalWidth - margin * (n - 1);
  var scale = actualTotalImagesWidth / totalImagesWidth;
  var lastX = leftMargin; // horizontal
  for (var i = 0; i < objs.length; i++) {
    var o = objs[i];
    var bounds = o.geometricBounds;
    var currHeight = bounds[2] - bounds[0];
    var newHeight = currHeight * scale;
    var newWidth = imagesWidth[i] * scale;
    o.geometricBounds = [
      startPosY,
      lastX,
      startPosY + newHeight,
      lastX + newWidth,
    ];
    o.fit(FitOptions.FILL_PROPORTIONALLY);
    lastX = lastX + newWidth + margin;
  }

  return;
}

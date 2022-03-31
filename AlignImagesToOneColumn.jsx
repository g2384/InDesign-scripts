//AlignImagesToOneColumn.jsx
//An InDesign JavaScript
/*  
@@@BUILDINFO@@@ "AlignImagesToOneColumn.jsx" 1.0.0 30 March 2022
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
        fitToWidth(objects);
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

function fitToWidth(objs) {
  var n = objs.length;
  var firstO = objs[0];
  var pg = firstO.parentPage;
  var guides = getGuide(pg, HorizontalOrVertical.HORIZONTAL);
  var startPos = guides[3];
  var endPos = guides[guides.length - 1];
  var imageTotalHeight = endPos - startPos;

  var leftMargin = pg.marginPreferences.left;
  var rightMargin = pg.marginPreferences.right;
  var pageW = pg.bounds[3] - pg.bounds[1];
  var viewW = pageW - rightMargin - leftMargin;

  var margin = 2.5;
  var stretchThreshold = (viewW - 11 * margin) / 12;
  stretchThreshold = viewW - stretchThreshold * 2; // change image's width to page width if its width is greater than this value

  var imagesHeight = [];
  for (var i = 0; i < objs.length; i++) {
    var bounds = objs[i].geometricBounds;
    imagesHeight.push(bounds[2] - bounds[0]);
  }

  var totalImagesHeight = sum(imagesHeight);
  var actualTotalImagesHeight = imageTotalHeight - margin * (n - 1);
  var scale = actualTotalImagesHeight / totalImagesHeight;
  var lastY = startPos; // vertical
  for (var i = 0; i < objs.length; i++) {
    var o = objs[i];
    var bounds = o.geometricBounds;
    var currWidth = bounds[3] - bounds[1];
    var newWidth = currWidth * scale;
    if (newWidth > stretchThreshold) {
      newWidth = viewW;
    }
    var newHeight = imagesHeight[i] * scale;
    var startX = viewW / 2 - newWidth / 2 + leftMargin;
    o.geometricBounds = [lastY, startX, lastY + newHeight, startX + newWidth];
    o.fit(FitOptions.FILL_PROPORTIONALLY);
    lastY = lastY + newHeight + margin;
  }

  return;
}

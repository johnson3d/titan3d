namespace Jither.OpenEXR.Attributes;

public static class AttributeNames
{
    /// <summary>
    /// Description of image channels stored in a part.
    /// </summary>
    public const string Channels = "channels";

    /// <summary>
    /// Specifies the compression method applied to the pixel data of all channels in a part.
    /// </summary>
    public const string Compression = "compression";

    /// <summary>
    /// An OpenEXR file may not have pixel data for all the pixels in the display window, or the file may have pixel data beyond the boundaries of the display window.
    /// The region for which pixel data are available is defined by a second axis-parallel rectangle in pixel space, the data window.
    /// </summary>
    public const string DataWindow = "dataWindow";

    /// <summary>
    /// The boundaries of an OpenEXR image are given as an axis-parallel rectangular region in pixel space, the display window.
    /// The display window is defined by the positions of the pixels in the upper left and lower right corners, (xMin, yMin) and (xMax, yMax).
    /// </summary>
    public const string DisplayWindow = "displayWindow";

    /// <summary>
    /// Specifies in what order the scan lines in the file are stored in the file (increasing Y, decreasing Y, or, for tiled images, also random Y).
    /// </summary>
    public const string LineOrder = "lineOrder";

    /// <summary>
    /// Width divided by height of a pixel when the image is displayed with the correct aspect ratio.
    /// A pixel’s width (height) is the distance between the centers of two horizontally (vertically) adjacent pixels on the display.
    /// </summary>
    public const string PixelAspectRatio = "pixelAspectRatio";

    /// <summary>
    /// With <seealso cref="ScreenWindowWidth"/> describes the perspective projection that produced the image. Programs that deal with images as purely
    /// two-dimensional objects may not be able so generate a description of a perspective projection. Those programs should set screenWindowWidth to 1,
    /// and screenWindowCenter to (0, 0).
    /// </summary>
    public const string ScreenWindowCenter = "screenWindowCenter";

    /// <summary>
    /// With <seealso cref="ScreenWindowCenter"/> describes the perspective projection that produced the image. Programs that deal with images as purely
    /// two-dimensional objects may not be able so generate a description of a perspective projection. Those programs should set screenWindowWidth to 1,
    /// and screenWindowCenter to (0, 0).
    /// </summary>
    public const string ScreenWindowWidth = "screenWindowWidth";

    /// <summary>
    /// The name attribute defines the name of each part. The name of each part must be unique. Names may contain ‘.’ characters to present a tree-like structure of the parts in a file.
    /// Required if the file is either MultiPart or NonImage.
    /// </summary>
    public const string Name = "name";

    /// <summary>
    /// Determines the size of the tiles and the number of resolution levels in the file.
    /// </summary>
    /// <remarks>
    /// The OpenEXR library ignores tile description attributes in scan line based files. The decision whether the file contains scan lines or tiles 
    /// is based on the value of bit 9 in the file’s version field, not on the presence of a tile description attribute.
    /// </remarks>
    public const string Tiles = "tiles";

    /// <summary>
    /// Part types are defined by the type attribute. There are four types:
    /// 
    /// 1. Scan line images: indicated by a type attribute of scanlineimage.
    /// 2. Tiled images: indicated by a type attribute of tiledimage.
    /// 3. Deep scan line images: indicated by a type attribute of deepscanline.
    /// 4. Deep tiled images: indicated by a type attribute of deeptile.
    /// 
    /// Required if the file is either MultiPart or NonImage.
    /// </summary>
    /// <remarks>
    /// This value must agree with the version field’s tile bit (9) and non-image (deep data) bit (11) settings.
    /// </remarks>
    public const string Type = "type";

    /// <summary>
    /// Version is required for deep data (deepscanline and deeptile) parts. If not specified for other parts, 1 is assumed.
    /// </summary>
    public const string Version = "version";

    /// <summary>
    /// Indicates the number of chunks in a part. Required if the multipart bit (12) is set.
    /// </summary>
    public const string ChunkCount = "chunkCount";

    /// <summary>
    /// Required for deep data (deepscanline and deeptile) parts. Note: Since the value of <c>maxSamplesPerPixel</c> maybe be unknown at the time of opening the file,
    /// the value “-1” is written to the file to indicate an unknown value. When the file is closed, this will be overwritten with the correct value. If file writing
    /// does not complete correctly due to an error, the value -1 will remain. In this case, the value must be derived by decoding each chunk in the part.
    /// </summary>
    public const string MaxSamplesPerPixel = "maxSamplesPerPixel";

    /// <summary>
    /// Specifies the view this part is associated with (mostly used for files which stereo views).
    /// 
    /// * A value of left indicate the part is associated with the left eye.
    /// * A value of right indicates the right eye
    /// 
    /// If there is no view attribute in the header, the entire part contains information not dependent on a particular eye.
    /// 
    /// This attribute can be used in the header for multi-part files.
    /// </summary>
    public const string View = "view";

    /// <summary>
    /// For RGB images, specifies the CIE (x,y) chromaticities of the primaries and the white point.
    /// </summary>
    public const string Chromaticities = "chromaticities";

    /// <summary>
    /// For RGB images, defines the luminance, in Nits (candelas per square meter) of the RGB value (1.0, 1.0, 1.0).
    /// </summary>
    /// <remarks>
    /// If the chromaticities and the whiteLuminance of an RGB image are known, then it is possible to convert the image’s pixels from RGB to CIE XYZ tristimulus values.
    /// </remarks>
    public const string WhiteLuminance = "whiteLuminance";

    /// <summary>
    /// Specifies the CIE (x,y) coordinates that should be considered neutral during color rendering. Pixels in the image file whose (x,y) coordinates match the
    /// adoptedNeutral value should be mapped to neutral values on the display.
    /// </summary>
    public const string AdoptedNeutral = "adoptedNeutral";

    /// <summary>
    /// Specifies the names of the CTL functions that implement the intended color rendering transforms for this image.
    /// </summary>
    public const string RenderingTransform = "renderingTransform";

    /// <summary>
    /// Specifies the names of the CTL functions that implement the intended look mod transforms for this image.
    /// </summary>
    public const string LookModTransform = "lookModTransform";

    /// <summary>
    /// Horizontal output density, in pixels per inch. The image’s vertical output density is xDensity * pixelAspectRatio.
    /// </summary>
    public const string XDensity = "xDensity";

    /// <summary>
    /// Name of the owner of the image.
    /// </summary>
    public const string Owner = "owner";

    /// <summary>
    /// Additional image information in human-readable form, for example a verbal description of the image.
    /// </summary>
    public const string Comments = "comments";

    /// <summary>
    /// The date when the image was created or captured, in local time, and formatted as <c>YYYY:MM:DD hh:mm:ss</c>, where <c>YYYY</c> is the year (4 digits, e.g. 2003),
    /// <c>MM</c> is the month (2 digits, 01, 02, … 12), <c>DD</c> is the day of the month (2 digits, 01, 02, … 31), <c>hh</c> is the hour (2 digits, 00, 01, … 23),
    /// <c>mm</c> is the minute, and <c>ss</c> is the second (2 digits, 00, 01, … 59).
    /// </summary>
    public const string CapDate = "capDate";

    /// <summary>
    /// Universal Coordinated Time (UTC), in seconds: UTC == local time + utcOffset
    /// </summary>
    public const string UtcOffset = "utcOffset";

    /// <summary>
    /// For images of real objects, the longitude where the image was recorded. Longitude is in degrees east of Greenwich.
    /// </summary>
    /// <remarks>
    /// For example, Kathmandu, Nepal is at longitude 85.317, latitude 27.717, altitude 1305.
    /// </remarks>
    public const string Longitude = "longitude";

    /// <summary>
    /// For images of real objects, the latitude where the image was recorded. Latitude is in degrees north of the equator.
    /// </summary>
    /// <remarks>
    /// For example, Kathmandu, Nepal is at longitude 85.317, latitude 27.717, altitude 1305.
    /// </remarks>
    public const string Latitude = "latitude";

    /// <summary>
    /// For images of real objects, the altitude where the image was recorded. Altitude is in meters above sea level.
    /// </summary>
    /// <remarks>
    /// For example, Kathmandu, Nepal is at longitude 85.317, latitude 27.717, altitude 1305.
    /// </remarks>
    public const string Altitude = "altitude";

    /// <summary>
    /// The camera’s focus distance, in meters.
    /// </summary>
    public const string Focus = "focus";

    /// <summary>
    /// Exposure time, in seconds.
    /// </summary>
    public const string Exposure = "exposure";

    /// <summary>
    /// The camera’s lens aperture, in f-stops (focal length of the lens divided by the diameter of the iris opening).
    /// </summary>
    public const string Aperture = "aperture";

    /// <summary>
    /// The ISO speed of the film or image sensor that was used to record the image.
    /// </summary>
    public const string IsoSpeed = "isoSpeed";

    /// <summary>
    /// If this attribute is present, the image represents an environment map. The attribute’s value defines how 3D directions are mapped to 2D pixel locations.
    /// </summary>
    public const string EnvMap = "envMap";

    /// <summary>
    /// For motion picture film frames. Identifies film manufacturer, film type, film roll and frame position within the roll.
    /// </summary>
    public const string KeyCode = "keyCode";

    /// <summary>
    /// Time and control code
    /// </summary>
    public const string TimeCode = "timeCode";

    /// <summary>
    /// Determines how texture map images are extrapolated. If an OpenEXR file is used as a texture map for 3D rendering, texture coordinates (0.0, 0.0) and (1.0, 1.0)
    /// correspond to the upper left and lower right corners of the data window. If the image is mapped onto a surface with texture coordinates outside the zero-to-one range,
    /// then the image must be extrapolated. This attribute tells the renderer how to do this extrapolation. The attribute contains either a pair of comma-separated keywords,
    /// to specify separate extrapolation modes for the horizontal and vertical directions; or a single keyword, to specify extrapolation in both directions
    /// (e.g. “clamp,periodic” or “clamp”). Extra white space surrounding the keywords is allowed, but should be ignored by the renderer (“clamp, black ” is equivalent to “clamp,black”).
    /// The keywords listed below are predefined; some renderers may support additional extrapolation modes:
    /// 
    /// * black - pixels outside the zero-to-one range are black
    /// * clamp - texture coordinates less than 0.0 and greater than 1.0 are clamped to 0.0 and 1.0 respectively.
    /// * periodic - the texture image repeats periodically
    /// * mirror - the texture image repeats periodically, but every other instance is mirrored
    /// </summary>
    public const string Wrapmodes = "wrapmodes";

    /// <summary>
    /// Defines the nominal playback frame rate for image sequences, in frames per second. Every image in a sequence should have a framesPerSecond attribute, and the attribute
    /// value should be the same for all images in the sequence. If an image sequence has no framesPerSecond attribute, playback software should assume that the frame rate for
    /// the sequence is 24 frames per second.
    /// </summary>
    /// <remarks>
    /// In order to allow exact representation of NTSC frame and field rates, framesPerSecond is stored as a rational number. A rational number is a pair of integers,
    /// <c>n</c> and <c>d</c>, that represents the value <c>n / d</c>.
    /// </remarks>
    public const string FramesPerSecond = "framesPerSecond";

    /// <summary>
    /// Defines the view names for multi-view image files. A multi-view image contains two or more views of the same scene, as seen from different viewpoints,
    /// for example a left-eye and a right-eye view for stereo displays. The multiView attribute lists the names of the views in an image, and a naming convention
    /// identifies the channels that belong to each view.
    /// </summary>
    public const string MultiView = "multiView";

    /// <summary>
    /// For images generated by 3D computer graphics rendering, a matrix that transforms 3D points from the world to the camera coordinate space of the renderer.
    /// The camera coordinate space is left-handed. Its origin indicates the location of the camera. The positive x and y axes correspond to the “right” and “up” directions in the rendered image.
    /// The positive z axis indicates the camera’s viewing direction. (Objects in front of the camera have positive z coordinates.)
    /// </summary>
    /// <remarks>
    /// Camera coordinate space in OpenEXR is the same as in Pixar’s Renderman.
    /// </remarks>
    public const string WorldToCamera = "worldToCamera";

    /// <summary>
    /// For images generated by 3D computer graphics rendering, a matrix that transforms 3D points from the world to the Normalized Device Coordinate (NDC) space of the renderer.
    /// NDC is a 2D coordinate space that corresponds to the image plane, with positive x and pointing to the right and y positive pointing down. The coordinates (0, 0) and(1, 1) correspond
    /// to the upper left and lower right corners of the OpenEXR display window.
    /// To transform a 3D point in word space into a 2D point in NDC space, multiply the 3D point by the worldToNDC matrix and discard the z coordinate.
    /// </summary>
    /// <remarks>
    /// NDC space in OpenEXR is the same as in Pixar’s Renderman.
    /// </remarks>
    public const string WorldToNDC = "worldToNDC";

    /// <summary>
    /// Specifies whether the pixels in a deep image are sorted and non-overlapping.
    /// </summary>
    /// <remarks>
    /// Note: this attribute can be set by application code that writes a file in order to tell applications that read the file whether the pixel data must be cleaned up prior to image
    /// processing operations such as flattening. The OpenEXR library does not verify that the attribute is consistent with the actual state of the pixels. Application software may assume
    /// that the attribute is valid, as long as the software will not crash or lock up if any pixels are inconsistent with the deepImageState attribute.
    /// </remarks>
    public const string DeepImageState = "deepImageState";

    /// <summary>
    /// If application software crops an image, then it should save the data window of the original, un-cropped image in the <c>originalDataWindow</c> attribute.
    /// </summary>
    public const string OriginalDataWindow = "originalDataWindow";

    /// <summary>
    /// Sets the quality level for images compressed with the DWAA or DWAB method.
    /// </summary>
    public const string DwaCompressionLevel = "dwaCompressionLevel";

    /// <summary>
    /// See <see cref="https://dl.acm.org/doi/abs/10.1145/3233085.3233086">A scheme for storing object ID manifests in openEXR images</see> for details.
    /// </summary>
    public const string IDManifest = "ID Manifest";
}

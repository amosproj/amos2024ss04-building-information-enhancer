import React from "react";
import { SvgIcon, SvgIconProps } from "@mui/material";
import { modifySvg } from "../../utils";

interface CustomSvgIconProps extends SvgIconProps {
  svgString: string;
  size?: number;
}

const CustomSvgIcon: React.FC<CustomSvgIconProps> = ({
  svgString,
  size = 32,
  ...props
}) => {
  const svgElement = modifySvg(svgString, size);

  return (
    <SvgIcon {...props} className="custom-svg-icon">
      <g dangerouslySetInnerHTML={{ __html: svgElement }} />
    </SvgIcon>
  );
};

export default CustomSvgIcon;

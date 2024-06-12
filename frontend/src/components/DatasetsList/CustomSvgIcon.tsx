import React from "react";
import { SvgIcon, SvgIconProps } from "@mui/material";

interface CustomSvgIconProps extends SvgIconProps {
  svgString: string;
}

const CustomSvgIcon: React.FC<CustomSvgIconProps> = ({
  svgString,
  ...props
}) => {
  return (
    <SvgIcon {...props}>
      <g dangerouslySetInnerHTML={{ __html: svgString }} />
    </SvgIcon>
  );
};

export default CustomSvgIcon;

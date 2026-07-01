import React from 'react';
import { Card } from 'antd';

interface PageCardProps {
  title?: React.ReactNode;
  children: React.ReactNode;
  className?: string;
}

export default function PageCard({ title, children, className }: PageCardProps) {
  return (
    <Card title={title} className={className}>
      {children}
    </Card>
  );
}
